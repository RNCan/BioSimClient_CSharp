/*
 * This file is part of the C# client for BioSIM Web API.
 *
 * Copyright (C) 2020-2022 Her Majesty the Queen in right of Canada
 * Authors: Mathieu Fortin and Jean-Francois Lavoie, 
 *          (Canadian Wood Fibre Centre, Canadian Forest Service)
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This library is distributed with the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public
 * License for more details.
 *
 * Please see the license at http://www.gnu.org/copyleft/lesser.html.
 */
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace biosimclient.Main
{
	/// <summary>
	/// A structure to host the model parameters.
	/// </summary>
	public sealed class BioSimParameterMap {

		/// <summary>
		/// The OrderedDictionary instance that contains the parameters.
		/// </summary>
		public OrderedDictionary InnerMap { get; private set; } = new();

		/// <summary>
		/// Add a parameter to the InnerMap.
		/// </summary>
		/// <param name="parameterName">a string</param>
		/// <param name="value">an object that stands for the value</param>
		public void AddParameter(string parameterName, object value)
		{
			if (IsNumber(value) || value.GetType() == typeof(string))
			InnerMap.Add(parameterName, value);
		else
				throw new ArgumentException("The value must be a String or a Number instance!");
		}


		private static bool IsNumber(object value)
		{
			return value is sbyte
					|| value is byte
					|| value is short
					|| value is ushort
					|| value is int
					|| value is uint
					|| value is long
					|| value is ulong
					|| value is float
					|| value is double
					|| value is decimal
					|| value is BigInteger;
		}

		/// <summary>
		/// Provide a customized string for this class.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new();
			String sep = "*";
			foreach (string key in InnerMap.Keys)
			{
				object value = InnerMap[key];
				string valueString = value == null ? "" : value.ToString().Trim();
				if (sb.Length == 0)
					sb.Append(key.Trim() + ":" + valueString);
				else
					sb.Append(sep + key.Trim() + ":" + valueString);
			}
			if (sb.Length == 0)
				return "null";
			else
				return sb.ToString();
		}

		/// <summary>
		/// Return true if the BioSimParmaeterMap instance is empty or false otherwise.
		/// </summary>
		/// <returns>a boolean</returns>
		public bool IsEmpty()
        {
			return InnerMap.Count == 0;
        }

	}
}
