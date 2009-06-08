/*
Copyright © Joan Charmant 2008.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/


#pragma once

using namespace System::Drawing;


namespace VideaPlayerServer {

	//------------------------------------------------------------------------------------------------------------------
	// Les valeurs des paramètres des filtres sont toujours entre -100 et +100, 
	// pour rester indépendant de l'implémentation et pour fournir une expérience identique pour tous les filtres.
	// Le mapping avec les valeurs attendues par la classe implémentant le filtre doit être fait dans la fonction l'appellant.
	//------------------------------------------------------------------------------------------------------------------

public ref class MirrorFilterParams
{
public:

	bool bActive;
	bool bMirrored;
};


public ref class FilterParams
{
public:
			FilterParams(void);
	void	Reset(void);

	bool	bActive;
	int		iValue;	
};


public ref class ImageFilter
{
	// ImageFilter
	// Contient la pile des filtre et maintient son état.

public:
	ImageFilter(void);


	void				ResetAllFilters(void);

	// Colors
	FilterParams^		GetColorsFilterParams(void);
	void				SetColorsFilterParams(FilterParams^ _ColorsFilterParams);
	Bitmap^				DoFilterColors(Bitmap^ _InputBitmap);
	Bitmap^				DoFilterColors(Bitmap^ _InputBitmap, FilterParams^ _ColorsFilterParams);

	// Brightness
	FilterParams^		GetBrightnessFilterParams(void);
	void				SetBrightnessFilterParams(FilterParams^ _BrightnessFilterParams);
	Bitmap^				DoFilterBrightness(Bitmap^ _InputBitmap);
	Bitmap^				DoFilterBrightness(Bitmap^ _InputBitmap, FilterParams^ _BrightnessFilterParams);

	// Contrast
	FilterParams^		GetContrastFilterParams(void);
	void				SetContrastFilterParams(FilterParams^ _ContrastFilterParams);
	Bitmap^				DoFilterContrast(Bitmap^ _InputBitmap);
	Bitmap^				DoFilterContrast(Bitmap^ _InputBitmap, FilterParams^ _ContrastFilterParams);

	// Sharpen
	FilterParams^		GetSharpenFilterParams(void);
	void				SetSharpenFilterParams(FilterParams^ _SharpenFilterParams);
	Bitmap^				DoFilterSharpen(Bitmap^ _InputBitmap);
	Bitmap^				DoFilterSharpen(Bitmap^ _InputBitmap, FilterParams^ _SharpenFilterParams);

	// Mirror
	void				ResetMirrorFilter();
	MirrorFilterParams^ GetMirrorFilterParams(void);
	void				SetMirrorFilterParams(MirrorFilterParams^ _MirrorFilterParams);
	void				DoFilterMirror(Bitmap^ _InputBitmap);

	// Edges
	FilterParams^		GetEdgesFilterParams(void);
	void				SetEdgesFilterParams(FilterParams^ _EdgesFilterParams);
	Bitmap^				DoFilterEdges(Bitmap^ _InputBitmap);
	Bitmap^				DoFilterEdges(Bitmap^ _InputBitmap, FilterParams^ _EdgesFilterParams);

	

private:
	
	FilterParams^		m_ColorsFilter;
	FilterParams^		m_BrightnessFilter;
	FilterParams^		m_ContrastFilter;
	FilterParams^		m_SharpenFilter;
	FilterParams^		m_EdgesFilter;

	MirrorFilterParams^		m_MirrorFilter;
	
	static log4net::ILog^ log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);
};


}