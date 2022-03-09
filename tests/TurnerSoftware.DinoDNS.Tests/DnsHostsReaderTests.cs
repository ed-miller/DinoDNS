﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TurnerSoftware.DinoDNS.Tests;

[TestClass]
public class DnsHostsReaderTests
{
	[DataTestMethod]
	[DataRow("# This is a comment", HostsTokenType.Comment, DisplayName = "Comment")]
	[DataRow("\r", HostsTokenType.NewLine, DisplayName = "Carriage Return")]
	[DataRow("\n", HostsTokenType.NewLine, DisplayName = "New Line")]
	[DataRow("\r\n", HostsTokenType.NewLine, DisplayName = "Carriage Return + New Line")]
	[DataRow(" ", HostsTokenType.Whitespace, DisplayName = "Space")]
	[DataRow("\t", HostsTokenType.Whitespace, DisplayName = "Tab")]
	[DataRow("127.0.0.1", HostsTokenType.HostOrAddress, DisplayName = "Address")]
	[DataRow("example.org", HostsTokenType.HostOrAddress, DisplayName = "Host")]
	public void ReadSingleToken(string hostsFile, HostsTokenType tokenType)
	{
		var reader = new DnsHostsReader(hostsFile);
		Assert.IsTrue(reader.NextToken(out var token), "No token");
		Assert.AreEqual(tokenType, token.TokenType);
		Assert.IsTrue(hostsFile.AsSpan().SequenceEqual(token.Value), "Sequences not equal");
	}
}
