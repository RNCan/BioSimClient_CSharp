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

namespace biosimclient.Main
{
	/// <summary>
	/// The interface for compatilibity with the BioSIM Web API.
	/// </summary>
    public interface IBioSimPlot
    {

		/// <summary>
		/// This method returns the elevation above sea level (m).
		/// </summary>
		/// <returns></returns>
		public double GetElevationM();

		/// <summary>
		/// This method returns the latitude of the plot in degrees.
		/// </summary>
		/// <returns></returns>
		public double GetLatitudeDeg();

		/// <summary>
		/// This method returns the longitude of the plot in degrees.
		/// </summary>
		/// <returns></returns>
		public double GetLongitudeDeg();

	}
}
