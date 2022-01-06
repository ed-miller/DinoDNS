﻿using System.Buffers.Binary;
using System.Collections;

namespace TurnerSoftware.DinoDNS.Protocol;

public readonly partial struct LabelSequence
{
	public const ushort PointerFlagByte = 0b11000000;
	public const byte PointerLength = sizeof(ushort);

	public struct Enumerator : IEnumerator<Label>
	{
		private readonly LabelSequence Value;
		private int Index;

		public Label Current { get; private set; }
		object IEnumerator.Current => Current;

		internal Enumerator(LabelSequence value)
		{
			Value = value;
			Index = value.IsByteSequence ? value.ByteValue.Offset : 0;
			Current = default;
		}

		private bool NextCharLabel()
		{
			var indexSlice = Value.CharValue[Index..];
			if (indexSlice.IsEmpty)
			{
				Current = default;
				return false;
			}

			var nextIndex = indexSlice.Span.IndexOf('.');
			var foundSeparator = nextIndex != -1;
			if (!foundSeparator)
			{
				nextIndex = indexSlice.Length;
			}

			var value = indexSlice[..nextIndex];
			Current = new Label(value);
			Index += (foundSeparator ? nextIndex + 1 : nextIndex);
			return true;
		}

		private bool NextByteLabel()
		{
			var seekableMemory = Value.ByteValue.Seek(Index);
			var fromPointer = false;

			var countOrPointer = seekableMemory.Current;
			while ((countOrPointer & PointerFlagByte) == PointerFlagByte)
			{
				//Pointers are a part of DNS message compression.
				//The first two bits say whether it is a pointer or not.
				//The next 14 bits represent the offset from the beginning of the message.
				var offset = BinaryPrimitives.ReadUInt16BigEndian(seekableMemory) & 0b00111111_11111111;
				// It seems that the ByteValue isn't actually the full segment of memory
				// The logic here is actually probably fine, something else is cropping the data
				seekableMemory = seekableMemory.Seek(offset);
				countOrPointer = seekableMemory.Current;
				fromPointer = true;
			}

			if (countOrPointer > 0 && countOrPointer <= Label.MaxLength)
			{
				seekableMemory = seekableMemory.SeekRelative(1).ReadNext(countOrPointer, out var value);
				Current = new Label(value, fromPointer);
				Index = seekableMemory.Offset;
				return true;
			}

			Current = default;
			return false;
		}

		public bool MoveNext()
		{
			if (Value.IsByteSequence)
			{
				return NextByteLabel();
			}
			return NextCharLabel();
		}

		public void Reset()
		{
			Index = 0;
			Current = default;
		}

		public void Dispose()
		{
		}
	}
}