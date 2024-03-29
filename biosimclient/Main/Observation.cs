﻿/*
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclient.Main
{
	/// <summary>
	/// A class for the observations contained in the BioSimDataSet class.
	/// </summary>
	public sealed class Observation : IComparable 
	{

		static List<int> comparableFields = new();

		/// <summary>
		/// The list of values contained in the Observation instance.
		/// </summary>
		public List<object> values;

		internal Observation(object[] obj)
		{
			values = new();
			values.AddRange(obj);
		}

		/// <inheritdoc />
		public int CompareTo(object o)
		{
			foreach (int index in comparableFields)
			{
				IComparable thisValue = (IComparable)values[index];
				IComparable thatValue = (IComparable)((Observation)o).values[index];
				int comparisonResult = thisValue.CompareTo(thatValue);
				if (comparisonResult < 0)
				{
					return -1;
				}
				else if (comparisonResult > 0)
				{
					return 1;
				}
			}
			return 0;
		}

		/// <summary>
		/// Convert this observation to an array of Object instances
		/// </summary>
		/// <returns>An array of objects</returns>
		public object[] ToArray()
		{
			return values.ToArray();
		}


		/// <summary>
		/// Check if two observations have the same values.
		/// </summary>
		/// <param name="obs">An Observation instance</param>
		/// <returns>a boolean: true if the instances are equal or false otherwise</returns>
		public bool IsEqualToThisObservation(Observation obs)
		{
			if (obs == null)
				return false;
			else
			{
				if (values.Count != obs.values.Count)
					return false;
				for (int i = 0; i < values.Count; i++)
				{
					Object thisValue = values[i];
					Object thatValue = obs.values[i];
					Type thisClass = thisValue.GetType();
					if (!thisClass.Equals(thatValue.GetType()))
						return false;
					else
					{
						if (Type.GetTypeCode(thisClass) == TypeCode.Double)
						{
							if (Math.Abs((double)thisValue - (double)thatValue) > 1E-8)
								return false;
						}
						else if (!thisValue.Equals(thatValue))
							return false;
					}
				}
				return true;
			}
		}
	}
}

