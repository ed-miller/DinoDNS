﻿using System.Collections;
using TurnerSoftware.DinoDNS.Internal;

namespace TurnerSoftware.DinoDNS.Protocol;

public readonly struct ResourceRecordCollection : IEquatable<ResourceRecordCollection>
{
	private readonly int ItemCount;
	private readonly SeekableReadOnlyMemory<byte> ByteValue;
	private readonly ResourceRecord[] ArrayValue;
	private readonly bool IsByteSequence;

	public ResourceRecordCollection(ResourceRecord[] value)
	{
		ItemCount = value.Length;
		ByteValue = ReadOnlyMemory<byte>.Empty;
		ArrayValue = value;
		IsByteSequence = false;
	}

	public ResourceRecordCollection(SeekableReadOnlyMemory<byte> value, int itemCount)
	{
		ItemCount = itemCount;
		ByteValue = value;
		ArrayValue = Array.Empty<ResourceRecord>();
		IsByteSequence = true;
	}

	public int Count => ItemCount;
	public bool Equals(ResourceRecordCollection other)
	{
		if (Count != other.Count)
		{
			return false;
		}

		var enumerator = GetEnumerator();
		var otherEnumerator = other.GetEnumerator();

		while (enumerator.MoveNext() && otherEnumerator.MoveNext())
		{
			if (!enumerator.Current.Equals(otherEnumerator.Current))
			{
				return false;
			}
		}

		return true;
	}

	public override bool Equals(object? obj) => obj is ResourceRecordCollection collection && Equals(collection);

	public override int GetHashCode() => ItemCount.GetHashCode();
	public Enumerator GetEnumerator() => new(this);

	public static implicit operator ResourceRecordCollection(ResourceRecord[] value) => new(value);
	public static bool operator ==(ResourceRecordCollection left, ResourceRecordCollection right) => left.Equals(right);
	public static bool operator !=(ResourceRecordCollection left, ResourceRecordCollection right) => !(left == right);

	public struct Enumerator : IEnumerator<ResourceRecord>
	{
		private readonly ResourceRecordCollection Value;
		private int Index;
		private DnsProtocolReader Reader;

		internal Enumerator(ResourceRecordCollection collection)
		{
			Value = collection;
			Index = 0;
			Current = default;
			Reader = Value.IsByteSequence ? new DnsProtocolReader(Value.ByteValue) : default;
		}

		public ResourceRecord Current { get; private set; }

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			if (Index == Value.ItemCount)
			{
				return false;
			}
			
			if (Value.IsByteSequence)
			{
				Reader = Reader.ReadResourceRecord(out var resourceRecord);
				Current = resourceRecord;
			}
			else
			{
				Current = Value.ArrayValue[Index];
			}

			Index++;
			return true;
		}

		public void Reset()
		{
			Index = 0;
			Current = default;
			Reader = Value.IsByteSequence ? new DnsProtocolReader(Value.ByteValue) : default;
		}

		public void Dispose()
		{
		}
	}
}
