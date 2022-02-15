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

namespace biosimclient.Main
{

	/// <summary>
	/// An enum defining the climate model.
	/// </summary>
	public enum ClimateModel
	{
	
		/// <summary>
		///  Hadley climate model
		/// </summary>
		Hadley,
		/// <summary>
		/// RCM 4 climate model (default climate model)
		/// </summary>
		RCM4,
		/// <summary>
		/// GCM 4 climate model
		/// </summary>
		GCM4
	}

	/// <summary>
	/// An enum that stands for the months of the year.
	/// </summary>
	public enum Month
	{
		/// <summary>
		/// January
		/// </summary>
		January,
		/// <summary>
		/// February
		/// </summary>
		February,
		/// <summary>
		/// March
		/// </summary>
		March,
		/// <summary>
		/// April
		/// </summary>
		April,
		/// <summary>
		/// May
		/// </summary>
		May,
		/// <summary>
		/// June
		/// </summary>
		June,
		/// <summary>
		/// July
		/// </summary>
		July,
		/// <summary>
		/// August
		/// </summary>
		August,
		/// <summary>
		/// September
		/// </summary>
		September,
		/// <summary>
		/// October
		/// </summary>
		October,
		/// <summary>
		/// November
		/// </summary>
		November,
		/// <summary>
		/// December
		/// </summary>
		December
	}

	/// <summary>
	/// An enum that stands for the climate change scenario (according to the IPCC)
	/// </summary>
	public enum RCP { 
		/// <summary>
		/// RCP 4.5
		/// </summary>
		RCP45, 
		/// <summary>
		/// RCP 8.5
		/// </summary>
		RCP85 }

	internal enum Variable { TN, T, TX, P, TD, H, WS, WD, R, Z, S, SD, SWE, WS2 }

	/// <summary>
	/// An enum variable that represents the 30-year period covered by the normals
	/// </summary>
	public enum Period
	{
		/// <summary>
		/// The 1951-1980 period
		/// </summary>
		FromNormals1951_1980,
		/// <summary>
		/// The 1961-1990 period
		/// </summary>
		FromNormals1961_1990,
		/// <summary>
		/// The 1971-2000 period
		/// </summary>
		FromNormals1971_2000,
		/// <summary>
		/// The 1981-2010 period
		/// </summary>
		FromNormals1981_2010,
		/// <summary>
		/// The 1991-2020 period
		/// </summary>
		FromNormals1991_2020,
		/// <summary>
		/// The 2001-2030 period
		/// </summary>
		FromNormals2001_2030,
		/// <summary>
		/// The 2011-2040 period
		/// </summary>
		FromNormals2011_2040,
		/// <summary>
		/// The 2021-2050 period
		/// </summary>
		FromNormals2021_2050,
		/// <summary>
		/// The 2031-2060 period
		/// </summary>
		FromNormals2031_2060,
		/// <summary>
		/// The 2041-2070 period
		/// </summary>
		FromNormals2041_2070,
		/// <summary>
		/// The 2051-2080 period
		/// </summary>
		FromNormals2051_2080,
		/// <summary>
		/// The 2061-2090 period
		/// </summary>
		FromNormals2061_2090,
		/// <summary>
		/// The 2071-2100 period
		/// </summary>
		FromNormals2071_2100
	}


	class EnumUtilities
	{
		static Dictionary<Month, int> numberOfDays = new();
		static Dictionary<RCP, string> rcpURL = new();
		static Dictionary<Variable, VariableDescriptor> variableDescriptors = new();
		static List<Variable> variablesForNormals = new();
		static List<String> fieldNamesForNormals = new();
		static Dictionary<Period, string> queryPartForNormals = new();

		private readonly static EnumUtilities Initilizer = new();

		private EnumUtilities()
		{
			numberOfDays.Add(Month.January, 31);
			numberOfDays.Add(Month.February, 28);
			numberOfDays.Add(Month.March, 31);
			numberOfDays.Add(Month.April, 30);
			numberOfDays.Add(Month.May, 31);
			numberOfDays.Add(Month.June, 30);
			numberOfDays.Add(Month.July, 31);
			numberOfDays.Add(Month.August, 31);
			numberOfDays.Add(Month.September, 30);
			numberOfDays.Add(Month.October, 31);
			numberOfDays.Add(Month.November, 30);
			numberOfDays.Add(Month.December, 31);

			rcpURL.Add(RCP.RCP45, "4_5");
			rcpURL.Add(RCP.RCP85, "8_5");

			variableDescriptors.Add(Variable.TN, new VariableDescriptor("TMIN_MN", false, "min air temperature"));
			variableDescriptors.Add(Variable.T, new VariableDescriptor("", false, "air temperature"));
			variableDescriptors.Add(Variable.TX, new VariableDescriptor("TMAX_MN", false, "max air temperature"));
			variableDescriptors.Add(Variable.P, new VariableDescriptor("PRCP_TT", true, "precipitation"));
			variableDescriptors.Add(Variable.TD, new VariableDescriptor("TDEX_MN", false, "temperature dew point"));
			variableDescriptors.Add(Variable.H, new VariableDescriptor("", false, "humidity"));
			variableDescriptors.Add(Variable.WS, new VariableDescriptor("", false, "wind speed"));
			variableDescriptors.Add(Variable.WD, new VariableDescriptor("", false, "wind direction"));
			variableDescriptors.Add(Variable.R, new VariableDescriptor("", true, "solar radiation"));
			variableDescriptors.Add(Variable.Z, new VariableDescriptor("", false, "atmospheric pressure"));
			variableDescriptors.Add(Variable.S, new VariableDescriptor("", true, "snow precipitation"));
			variableDescriptors.Add(Variable.SD, new VariableDescriptor("", false, "snow depth accumulation"));
			variableDescriptors.Add(Variable.SWE, new VariableDescriptor("", true, "snow water equivalent"));
			variableDescriptors.Add(Variable.WS2, new VariableDescriptor("", false, "wind speed at 2 m"));

			variablesForNormals.Add(Variable.TN);
			variablesForNormals.Add(Variable.TX);
			variablesForNormals.Add(Variable.P);

			foreach(Variable v in variablesForNormals)
            {
				fieldNamesForNormals.Add(GetVariableDescriptorForThisVariable(v).FieldName);
            }

			queryPartForNormals.Add(Period.FromNormals1951_1980, "period=1951_1980");
			queryPartForNormals.Add(Period.FromNormals1961_1990, "period=1961_1990");
			queryPartForNormals.Add(Period.FromNormals1971_2000, "period=1971_2000");
			queryPartForNormals.Add(Period.FromNormals1981_2010, "period=1981_2010");
			queryPartForNormals.Add(Period.FromNormals1991_2020, "period=1991_2020");
			queryPartForNormals.Add(Period.FromNormals2001_2030, "period=2001_2030");
			queryPartForNormals.Add(Period.FromNormals2011_2040, "period=2011_2040");
			queryPartForNormals.Add(Period.FromNormals2021_2050, "period=2021_2050");
			queryPartForNormals.Add(Period.FromNormals2031_2060, "period=2031_2060");
			queryPartForNormals.Add(Period.FromNormals2041_2070, "period=2041_2070");
			queryPartForNormals.Add(Period.FromNormals2051_2080, "period=2051_2080");
			queryPartForNormals.Add(Period.FromNormals2061_2090, "period=2061_2090");
			queryPartForNormals.Add(Period.FromNormals2071_2100, "period=2071_2100");
		}


		public static int GetNumberOfDaysInThisMonth(Month m)
		{
			return numberOfDays[m];
		}

		public static string GetURLStringForThisRCP(RCP r)
		{
			return rcpURL[r];
		}

		public static VariableDescriptor GetVariableDescriptorForThisVariable(Variable v)
        {
			return variableDescriptors[v];
        }

		internal static List<Variable> GetVariablesForNormals()
        {
			return variablesForNormals;
        }

		internal static string GetQueryPartForThisPeriod(Period p)
		{
			return queryPartForNormals[p];
        }

		internal static List<string> GetFieldNamesForNormals()
        {
			return fieldNamesForNormals;
        }
	}

	/// <summary>
	/// A descriptor of the field.
	/// </summary>
	public sealed class VariableDescriptor
    {
		/// <summary>
		/// The field name.
		/// </summary>
		public string FieldName { get; private set; }

		/// <summary>
		/// A boolean that is true if the field is additive or false otherwise
		/// </summary>
		public bool Additive { get; private set; }

		/// <summary>
		/// A string describing the field.
		/// </summary>
		public string Description { get; private set; }

		internal VariableDescriptor(string fieldName, bool additive, string description)
        {
			FieldName = fieldName;
			Additive = additive;
			Description = description;
        }
    }

}





