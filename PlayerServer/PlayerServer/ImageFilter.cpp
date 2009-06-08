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



#include "StdAfx.h"
#include "ImageFilter.h"


namespace VideaPlayerServer 
{

ImageFilter::ImageFilter(void)
{
	m_ColorsFilter		= gcnew FilterParams();
	m_BrightnessFilter	= gcnew FilterParams();
	m_ContrastFilter	= gcnew FilterParams();
	m_SharpenFilter		= gcnew FilterParams();
	m_EdgesFilter		= gcnew FilterParams(); 
	m_MirrorFilter		= gcnew MirrorFilterParams();
		
	ResetAllFilters();
}

void ImageFilter::ResetAllFilters(void)
{
	m_ColorsFilter->Reset();
	m_BrightnessFilter->Reset();
	m_ContrastFilter->Reset();
	m_SharpenFilter->Reset();
	m_EdgesFilter->Reset();

	ResetMirrorFilter();
}

#pragma region FilterParam
FilterParams::FilterParams()
{
	Reset();
}
//---------------------------------------------------------------------------------------------------------------------
void FilterParams::Reset()
{
	bActive	= false;
	iValue	= 0;
}
#pragma endregion


#pragma region MIRROR
void ImageFilter::ResetMirrorFilter()
{
	m_MirrorFilter->bActive		= false;
	m_MirrorFilter->bMirrored	= false;
}
MirrorFilterParams^ ImageFilter::GetMirrorFilterParams(void)
{
	MirrorFilterParams^ mfp = gcnew MirrorFilterParams();

	mfp->bActive	= m_MirrorFilter->bActive;
	mfp->bMirrored	= m_MirrorFilter->bMirrored;

	return mfp;
}
void ImageFilter::SetMirrorFilterParams(MirrorFilterParams^ _MirrorFilterParams)
{
	m_MirrorFilter->bActive = _MirrorFilterParams->bActive;
	m_MirrorFilter->bMirrored = _MirrorFilterParams->bMirrored;
}
void ImageFilter::DoFilterMirror(Bitmap^ _InputBitmap)
{
	// Dans les deux sens, le filtre à appliquer est le même.
	// On a donc pas besoin de se servir de bMirrored ici.
	// Mais on le garde pour la gestion de la coche du menu.
	_InputBitmap->RotateFlip( System::Drawing::RotateFlipType::RotateNoneFlipX );
}
#pragma endregion

#pragma region COLORS
//---------------------------------------------------------------------------------------------------------------------
//ColorsFilterParams::ColorsFilterParams()
//{
//	Reset();
//}
////---------------------------------------------------------------------------------------------------------------------
//void ColorsFilterParams::Reset()
//{
//	bActive	= false;
//	iSaturationFactor = 0;
//}
////---------------------------------------------------------------------------------------------------------------------
FilterParams^ ImageFilter::GetColorsFilterParams(void)
{
	FilterParams^ cfp = gcnew FilterParams();

	cfp->bActive	= m_ColorsFilter->bActive;
	cfp->iValue		= m_ColorsFilter->iValue;
	return cfp;
}
//---------------------------------------------------------------------------------------------------------------------
void ImageFilter::SetColorsFilterParams(FilterParams^ _ColorsFilterParams)
{
	m_ColorsFilter->bActive = _ColorsFilterParams->bActive;
	m_ColorsFilter->iValue  = _ColorsFilterParams->iValue;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterColors(Bitmap^ _InputBitmap)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée en batch lors du filtrage de toutes les frames de la vidéo.
	// Les paramètres utilisés sont ceux de l'objet global.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//-----------------------------------------------------------
	// On a deux filtres à passer. 
	// L'important est de toujours les passer dans le même ordre.
	//-----------------------------------------------------------

	//--------------------------------------
	// Création des filtre
	// Mapping : [-100, +100] => [-1.0, 1.0]
	//--------------------------------------
	AForge::Imaging::Filters::SaturationCorrection^ filter = gcnew AForge::Imaging::Filters::SaturationCorrection((double)m_ColorsFilter->iValue/100);	

	// Application
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterColors(Bitmap^ _InputBitmap, FilterParams^ _ColorsFilterParams)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée indépendamment de l'objet global, pour preview par exemple.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//-----------------------------------------------------------
	// On a deux filtres à passer. 
	// L'important est de toujours les passer dans le même ordre.
	//-----------------------------------------------------------

	//--------------------------------------
	// Création des filtre
	// Mapping : [-100, +100] => [-1.0, 1.0]
	//--------------------------------------
	AForge::Imaging::Filters::SaturationCorrection^ filter = gcnew AForge::Imaging::Filters::SaturationCorrection((double)_ColorsFilterParams->iValue/100);	
		
	// Application
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
#pragma endregion

#pragma region BRIGHTNESS
//---------------------------------------------------------------------------------------------------------------------
FilterParams^ ImageFilter::GetBrightnessFilterParams(void)
{
	FilterParams^ cfp = gcnew FilterParams();

	cfp->bActive	= m_BrightnessFilter->bActive;
	cfp->iValue		= m_BrightnessFilter->iValue;
	return cfp;
}
//---------------------------------------------------------------------------------------------------------------------
void ImageFilter::SetBrightnessFilterParams(FilterParams^ _BrightnessFilterParams)
{
	m_BrightnessFilter->bActive = _BrightnessFilterParams->bActive;
	m_BrightnessFilter->iValue  = _BrightnessFilterParams->iValue;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterBrightness(Bitmap^ _InputBitmap)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée en batch lors du filtrage de toutes les frames de la vidéo.
	// Les paramètres utilisés sont ceux de l'objet global.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//-----------------------------------------------------------
	// On a deux filtres à passer. 
	// L'important est de toujours les passer dans le même ordre.
	//-----------------------------------------------------------

	//--------------------------------------------------------
	// Création des filtre
	// Mapping : [-100, +100] => [-0.25, 0.25] 
	// (Valeurs limites acceptées par le filtre : [-1.0, 1.0])
	//--------------------------------------------------------
	double fValue = ((double)m_BrightnessFilter->iValue / 100) / 4;

	AForge::Imaging::Filters::BrightnessCorrection^ filter = gcnew AForge::Imaging::Filters::BrightnessCorrection(fValue);	

	// Application
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterBrightness(Bitmap^ _InputBitmap, FilterParams^ _BrightnessFilterParams)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée indépendamment de l'objet global, pour preview par exemple.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//-----------------------------------------------------------
	// On a deux filtres à passer. 
	// L'important est de toujours les passer dans le même ordre.
	//-----------------------------------------------------------

	//--------------------------------------------------------
	// Création des filtre
	// Mapping : [-100, +100] => [-0.25, 0.25] 
	// (Valeurs limites acceptées par le filtre : [-1.0, 1.0])
	//--------------------------------------------------------
	double fValue = ((double)_BrightnessFilterParams->iValue / 100) / 4;

	AForge::Imaging::Filters::BrightnessCorrection^ filter = gcnew AForge::Imaging::Filters::BrightnessCorrection(fValue);	
		
	// Application
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
#pragma endregion

#pragma region CONTRAST
//---------------------------------------------------------------------------------------------------------------------
//ContrastFilterParams::ContrastFilterParams()
//{
//	Reset();
//}
////---------------------------------------------------------------------------------------------------------------------
//void ContrastFilterParams::Reset()
//{
//	bActive	= false;
//	fContrastFactor = 1.25f;
//	fBrightnessFactor = 0.1f;
//}
//---------------------------------------------------------------------------------------------------------------------
FilterParams^ ImageFilter::GetContrastFilterParams(void)
{
	FilterParams^ cfp = gcnew FilterParams();

	cfp->bActive			= m_ContrastFilter->bActive;
	cfp->iValue				= m_ContrastFilter->iValue;

	return cfp;
}
//---------------------------------------------------------------------------------------------------------------------
void ImageFilter::SetContrastFilterParams(FilterParams^ _ContrastFilterParams)
{
	m_ContrastFilter->bActive = _ContrastFilterParams->bActive;
	m_ContrastFilter->iValue  = _ContrastFilterParams->iValue;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterContrast(Bitmap^ _InputBitmap)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée en batch lors du filtrage de toutes les frames de la vidéo.
	// Les paramètres utilisés sont ceux de l'objet global.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//-----------------------------------------------------------
	// On a deux filtres à passer. 
	// L'important est de toujours les passer dans le même ordre.
	//-----------------------------------------------------------

	//--------------------------------------------------------
	// Création des filtre
	// Mapping : [-100, +100] => [0.45, 3.0], valeur normale à 1.0
	// (Valeurs limites acceptées par le filtre : [0.0, 5.0])
	//--------------------------------------------------------
	double fValue = ((double)m_ContrastFilter->iValue / 100) + 1;

	AForge::Imaging::Filters::ContrastCorrection^ filter = gcnew AForge::Imaging::Filters::ContrastCorrection(fValue);	

	// Application des filtres
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterContrast(Bitmap^ _InputBitmap, FilterParams^ _ContrastFilterParams)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée indépendamment de l'objet global, pour preview par exemple.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//-----------------------------------------------------------
	// On a deux filtres à passer. 
	// L'important est de toujours les passer dans le même ordre.
	//-----------------------------------------------------------

	//--------------------------------------------------------
	// Création des filtre
	// Mapping : [-100, +100] => [0.45, 3.0], valeur normale à 1.0
	// (Valeurs limites acceptées par le filtre : [0.0, 5.0])
	//--------------------------------------------------------
	double fValue = ((double)_ContrastFilterParams->iValue / 100) + 1;

	AForge::Imaging::Filters::ContrastCorrection^ filter = gcnew AForge::Imaging::Filters::ContrastCorrection(fValue);
		
	// Application des filtres
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
//---------------------------------------------------------------------------------------------------------------------
#pragma endregion

#pragma region SHARPEN
//---------------------------------------------------------------------------------------------------------------------
FilterParams^ ImageFilter::GetSharpenFilterParams(void)
{
	FilterParams^ mfp = gcnew FilterParams();

	mfp->bActive	= m_SharpenFilter->bActive;
	mfp->iValue		= m_SharpenFilter->iValue;
	
	return mfp;
}
//---------------------------------------------------------------------------------------------------------------------
void ImageFilter::SetSharpenFilterParams(FilterParams^ _SharpenFilterParams)
{
	m_SharpenFilter->bActive = _SharpenFilterParams->bActive;
	m_SharpenFilter->iValue = _SharpenFilterParams->iValue;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterSharpen(Bitmap^ _InputBitmap)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée en batch lors du filtrage de toutes les frames de la vidéo.
	// Les paramètres utilisés sont ceux de l'objet global.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//----------------------------------
	// Création du filtre
	// Entrée : [0;100]
	// Sortie : 
	// fSigma : [0;3]
	// iSize  : [1;21] pixels ?  
	//----------------------------------
	double  fSigma	= (double)m_SharpenFilter->iValue / 40;
	int		iSize	= 5;
	
	AForge::Imaging::Filters::SharpenEx^ filter = gcnew AForge::Imaging::Filters::SharpenEx(fSigma, iSize);
		
	// Application du filtre
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterSharpen(Bitmap^ _InputBitmap, FilterParams^ _SharpenFilterParams)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée indépendamment de l'objet global, pour preview par exemple.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//----------------------------------
	// Création du filtre
	// 2 params : 
	// fSigma ( défaut : 1.5)
	// iSize  ( défaut : 5.0)
	//----------------------------------
	double  fSigma	= (double) _SharpenFilterParams->iValue / 40;
	int		iSize	= 5;

	AForge::Imaging::Filters::SharpenEx^ filter = gcnew AForge::Imaging::Filters::SharpenEx(fSigma, iSize);
		
	// Application du filtre
	tmp = filter->Apply(_InputBitmap);

	// Lâcher l'originale.
	delete _InputBitmap;
	
	return tmp;
}
//---------------------------------------------------------------------------------------------------------------------
#pragma endregion

#pragma region EDGES
//---------------------------------------------------------------------------------------------------------------------
FilterParams^ ImageFilter::GetEdgesFilterParams(void)
{
	FilterParams^ mfp = gcnew FilterParams();

	mfp->bActive	= m_EdgesFilter->bActive;
	mfp->iValue		= m_EdgesFilter->iValue;
	
	return mfp;
}
//---------------------------------------------------------------------------------------------------------------------
void ImageFilter::SetEdgesFilterParams(FilterParams^ _EdgesFilterParams)
{
	m_EdgesFilter->bActive = _EdgesFilterParams->bActive;
	m_EdgesFilter->iValue = _EdgesFilterParams->iValue;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterEdges(Bitmap^ _InputBitmap)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée en batch lors du filtrage de toutes les frames de la vidéo.
	// Les paramètres utilisés sont ceux de l'objet global.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//----------------------------------
	// Filtre sans params
	//----------------------------------
	AForge::Imaging::Filters::DifferenceEdgeDetector^ filter = gcnew AForge::Imaging::Filters::DifferenceEdgeDetector();
		
	// Application du filtre
	tmp = filter->Apply(_InputBitmap);


	// Repasser en RGB
	AForge::Imaging::Filters::GrayscaleToRGB^ toRGB = gcnew AForge::Imaging::Filters::GrayscaleToRGB();

	Bitmap^ tmp2 = toRGB->Apply(tmp);


	// Lâcher l'originale.
	delete _InputBitmap;
	delete tmp;
	
	return tmp2;
}
//---------------------------------------------------------------------------------------------------------------------
Bitmap^ ImageFilter::DoFilterEdges(Bitmap^ _InputBitmap, FilterParams^ _EdgesFilterParams)
{
	//----------------------------------------------------------------------------------------------
	// Fonction destinée à être utilisée indépendamment de l'objet global, pour preview par exemple.
	//----------------------------------------------------------------------------------------------
	Bitmap^ tmp;

	//----------------------------------
	// Filtre sans params
	//----------------------------------
	AForge::Imaging::Filters::DifferenceEdgeDetector^ filter = gcnew AForge::Imaging::Filters::DifferenceEdgeDetector();
		
	// Application du filtre
	tmp = filter->Apply(_InputBitmap);

	// Repasser en RGB
	AForge::Imaging::Filters::GrayscaleToRGB^ toRGB = gcnew AForge::Imaging::Filters::GrayscaleToRGB();

	Bitmap^ tmp2 = toRGB->Apply(tmp);

	// Lâcher l'originale.
	delete _InputBitmap;
	delete tmp;
	
	return tmp2;
}
//---------------------------------------------------------------------------------------------------------------------
#pragma endregion

}
