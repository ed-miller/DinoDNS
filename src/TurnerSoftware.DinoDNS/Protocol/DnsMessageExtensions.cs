﻿namespace TurnerSoftware.DinoDNS.Protocol;

public static class DnsMessageExtensions
{
	public static DnsMessage WithQuestions(this in DnsMessage message, Question[] questions)
	{
		return message with
		{
			Header = message.Header with
			{
				QuestionRecordCount = (ushort)questions.Length,
			},
			Questions = questions
		};
	}

	public static DnsMessage WithAnswers(this in DnsMessage message, ResourceRecord[] answers)
	{
		return message with
		{
			Header = message.Header with
			{
				AnswerRecordCount = (ushort)answers.Length,
			},
			Answers = answers
		};
	}

	public static DnsMessage WithAuthorities(this in DnsMessage message, ResourceRecord[] authorities)
	{
		return message with
		{
			Header = message.Header with
			{
				AuthorityRecordCount = (ushort)authorities.Length,
			},
			Authorities = authorities
		};
	}

	public static DnsMessage WithAdditionalRecords(this in DnsMessage message, ResourceRecord[] additionalRecords)
	{
		return message with
		{
			Header = message.Header with
			{
				AdditionalRecordCount = (ushort)additionalRecords.Length,
			},
			AdditionalRecords = additionalRecords
		};
	}
}
