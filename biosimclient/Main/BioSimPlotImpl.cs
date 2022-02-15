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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace biosimclient.Main
{
	/// <summary>
	/// A basic implementation of the IBioSimPlot interface.
	/// </summary>
	public class BioSimPlotImpl : IBioSimPlot
	{
		/// <summary>
		/// The elevation above sea level (m)
		/// </summary>
		public double ElevationM { get; private set; }
		/// <summary>
		/// The latitude in degrees.
		/// </summary>
		public double Latitude { get; private set; }
		/// <summary>
		/// The longitude in degrees.
		/// </summary>
		public double Longitude { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="latitudeDeg"></param>
		/// <param name="longitudeDeg"></param>
		/// <param name="elevationM"></param>
		public BioSimPlotImpl(double latitudeDeg, double longitudeDeg, double elevationM)
		{
			Latitude = latitudeDeg;
			Longitude = longitudeDeg;
			ElevationM = elevationM;
		}

		/// <summary>
		/// Provide a customized string for this class.
		/// </summary>
		/// <returns></returns>
		public override string ToString() { return Latitude + "_" + Longitude + "_" + ElevationM; }

		/// <inheritdoc />
		public double GetElevationM()
        {
            return ElevationM;
        }

		/// <inheritdoc />
		public double GetLatitudeDeg()
        {
            return Latitude;
        }

		/// <inheritdoc />
		public double GetLongitudeDeg()
        {
            return Longitude;
        }


		bool Equals(BioSimPlotImpl otherPlot)
		{
			if (Math.Abs(Latitude - otherPlot.Latitude) < 1E-8)
				if (Math.Abs(Longitude - otherPlot.Longitude) < 1E-8)
					if (Math.Abs(ElevationM - otherPlot.ElevationM) < 1E-8)
						return true;
			return false;
		}

	}
}
