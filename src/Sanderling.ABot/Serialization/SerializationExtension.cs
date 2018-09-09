using System;
using System.Collections.Generic;
using BotEngine;

namespace Sanderling.ABot.Serialization
{
	public static class SerializationExtension
	{
		public static KeyValuePair<Deserialization, T> DeserializeIfDifferent<T>(
			this string newValueSerial,
			KeyValuePair<Deserialization, T> oldValue)
		{
			if (newValueSerial == oldValue.Key?.Serial)
				return oldValue;

			Exception exception = null;
			var newValueStruct = default(T);

			try
			{
				if (null != newValueSerial)
					newValueStruct = newValueSerial.DeserializeFromString<T>();
			}
			catch (Exception e)
			{
				exception = e;
			}

			return new KeyValuePair<Deserialization, T>(
				new Deserialization {Serial = newValueSerial, Exception = exception}, newValueStruct);
		}
	}
}