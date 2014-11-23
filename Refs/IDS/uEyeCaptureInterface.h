// The uEye Capture Device Filter supports a range of standard interfaces.
// These interfaces are:
// The Filter exposes:		IAMVideoProcAmp
//							IAMVideoControl
//							IAMDroppedFrames
//							IAMFilterMiscFlags
//							IKsPropertySet
//							IuEyeCapture		    - specific uEye interface
//							IuEyeCaptureEx          - specific uEye interface
//                          IuEyeAutoFeatures	    - specific uEye interface
//							IuEyeFaceDetection      - specific uEye interface
//							IuEyeImageStabilization - specific uEye interface
//							IuEyeSensorAWB          - specific uEye interface
//							IuEyeAutoContrast       - specific uEye interface
//							IuEyeAutoBacklight      - specific uEye interface
//							IuEyeAntiFlicker        - specific uEye interface
//							IuEyeScenePreset        - specific uEye interface
//							IuEyeDigitalZoom        - specific uEye interface
//							IuEyeFocus				- specific uEye interface
//							IuEyeSaturation         - specific uEye interface
//							IuEyeSharpness          - specific uEye interface
//							IuEyeColorTemperature   - specific uEye interface
//							IuEyeTriggerDebounce    - specific uEye interface
//							IuEyePhotometry         - specific uEye interface
//							IuEyeAutoFramerate      - specific uEye interface
//							IuEyeFlash              - specific uEye interface
//							IuEyeResample           - specific uEye interface
//                          IuEyeTrigger            - specific uEye interface
//							ISpecifyPropertyPages
// The Capture Pin exposes:	IAMCameraControl
//							IKsPropertySet
//							IAMStreamConfig
//							IuEyeCapturePin		    - specific uEye interface
//                          IuEyeAOI                - specific uEye interface
//							IuEyeScaler				- specific uEye interface
//							IuEyeIO					- specific uEye interface
//							IuEyeEvent				- specific uEye interface
//							IuEyeDeviceFeature		- specific uEye interface
//							IuEyeHotPixel			- specific uEye interface
//							IuEyeCameraLUT			- specific uEye interface
//							IuEyeEdgeEnhancement	- specific uEye interface	
//							IuEyeAutoParameter		- specific uEye interface
//							IuEyeImageFormat		- specific uEye interface
//							IuEyeColorConverter		- specific uEye interface
//
//							ISpecifyPropertyPages
// Some functionalities of the cameras are not mentioned in this standards.
// Therefore this file defines some additional interfaces, providing control
// over the missing features.

#ifndef _UEYE_CAPTURE_INTERFACE_
#define _UEYE_CAPTURE_INTERFACE_

#include "uEye.h"

#include <initguid.h>

const char uEyeCaptureInterfaceVersion[] = "3.0.16";


// {67030826-2EE0-44e7-BE1A-6A3BDB5B47FE}
DEFINE_GUID(IID_IuEyeCapturePin, 
            0x67030826, 0x2ee0, 0x44e7, 0xbe, 0x1a, 0x6a, 0x3b, 0xdb, 0x5b, 0x47, 0xfe);

// ============================================================================
/*! \defgroup IuEyeCapturePin uEye Capture Pin Interface
 *  Proprietary interface for extra functionality exposed by the capture pin.
 *  It controls mainly the pixelclock settings for sensor read in
 * \{
 */
// ============================================================================
interface IuEyeCapturePin : public IUnknown
{
    /*!
	 *  \brief  Returns the sum (in MHz) of pixelclock.
	 *	\param 	lClock      Receives the overall pixelclock sum.
	 *	\return	HRESULT     0 on success, error code otherwise.
	 *	\see    GetPixelClock
	 */
	STDMETHOD(GetUsedBandwith)(long *plClock) = 0;

    /*!
    *   \brief  Returns the pixelclock for the connected camera.
    *	\param 	plClock     Receives the current pixel clock.
    *	\return	HRESULT     0 on success, error code otherwise.
    *	\see    GetPixelClockRange, SetPixelClock
    */
    STDMETHOD(GetPixelClock)(long *plClock) = 0;

    /*!
	* \brief	Returns the min, max and default value for the pixelclock.
     *	\param 	plMin       Receives the minimum possible pixel clock.
     *	\param 	plMax       Receives the maximum possible pixel clock.
	*\param	plDefault   	Receives the default pixel clock value.
	*\return	HRESULT     0 on success, error code otherwise.
	*\see    	GetPixelClock, SetPixelClock
     */
    STDMETHOD(GetPixelClockRange)(long *plMin, long *plMax, long *plDefault) = 0;

    /*!
	* \brief	Sets the Pixelclock (in MHz) for the connected device.
     *	\param 	lClock      The pixel clock in MHz to set.
     *	\return	HRESULT     0 on success, CO_E_NOT_SUPPORTED if not supported, error code otherwise.
     *	\see    GetPixelClock, GetPixelClockRange
     */
    STDMETHOD(SetPixelClock)(long lClock) = 0;

    /*!
	* \brief	Queries which color mode to use when RGB8 mediatype is selected.
     *	\param 	plMode      Receives the currently selected RGB8 colormode.
     *	\return	HRESULT     0 on success, error code otherwise.
     *	\see    SetRGB8ColorMode
     */
    STDMETHOD(GetRGB8ColorMode)(long *plMode) = 0;

    /*!
	* \brief	Determines which color mode to use when RGB8 mediatype is selected.
	*
     *      Specifies whether Y8 or raw bayer pattern is used with RGB8 mode
     *		possible values are 11 for raw bayer pattern (on bayer color
     *      cameras), or 6 for monochrome images.
     *	\param 	lMode       Specifies the color mode used for RGB8 mode.
     *	\return	HRESULT     0 on success, error code otherwise.
     *	\see    GetRGB8ColorMode
     */
    STDMETHOD(SetRGB8ColorMode)(long lMode) = 0;

	/*! 
	* \brief	Queries the current possible min, max and interval for exposure time.
	*
     *      Gets the actual min, max and interval values for exposure time.
     *      Values are given in us unit.
	 *	\param 	plMinExp    Receives the minimum possible exposure time.
	 *	\param 	plMaxExp    Receives the maximum possible exposure time.
     *	\param 	plInterval  Receives the current possible step width.
     *	\return	HRESULT     0 on success, error code otherwise.
	 *	\see    GetExposureTime, SetExposureTime, SetPixelClock
     *
     *  \note   This range may change depending on framerate and pixelclock
     *          settings.
	 */
	STDMETHOD(GetExposureRange)(long *plMinExp, long *plMaxExp, long *plInterval) = 0;

    /*!
	* \brief	Queries the current exposure time
     *	\param 	plExp       Receives the current exposure time in us.
     *	\return	HRESULT     0 on success, error code otherwise.
     *	\see    GetExposureRange, SetExposureTime
     */
    STDMETHOD(GetExposureTime)(long *plExp ) = 0;

    /*!
	* \brief	Sets the exposure time of the camera.
     *		This function sets the exposure time in units of Microseconds and
     *      thus allows a finer exposure time granularity than the function of
     *      the IAMCameraControl Interface does. (2^n seconds vs. x us).
     *	\param 	lExp        Specifies the exposure time to use (in us).
     *	\return	HRESULT     0 on success, error code otherwise.
     *	\see    GetExposureTime, GetExposureRange
     */
    STDMETHOD(SetExposureTime)(long lExp) = 0;
};

/*!
 * \}
 */	// end of group uEyeCaptureInterface

//structs needed by some functions
#ifndef DS_EXPORT
#   define DS_EXPORT
#   ifdef CAMERAINFO
#       undef CAMERAINFO
#   endif
typedef struct
{
  char          SerNo[12];    /*!< \brief 	camera's serial number e.g. "12345-1234" */
  char          ID[20];       /*!< \brief 	 manufacturer specific string e.g. "IDS GmbH" */
  char          Version[10];  /*!< \brief 	 camera's version e.g. "V1.00" */
  char          Date[12];     /*!< \brief 	date of qc  e.g. "11.11.1999" */
  unsigned char Select;       /*!< \brief 	contains board select number for multi board support */
  unsigned char Type;         /*!< \brief 	contains board type */
  char          Reserved[8];  /*!< \brief 	reserved for future use */
} CAMERAINFO, *PCAMERAINFO;

//#   ifdef _SENSORINFO
//#       undef _SENSORINFO
//#   endif
//#   ifdef SENSORINFO
//#       undef SENSORINFO
//#   endif
//
//#   ifndef _SENSORINFO
//typedef struct _SENSORINFO
//{
//  WORD          SensorID;           /*!< \brief 	camera's sensor id e.g. IS_SENSOR_UI121X_C */
//  char          strSensorName[32];  /*!< \brief 	human readable name of the sensor e.g. "UI-121X-C" */
//  char          nColorMode;         /*!< \brief 	indicates monochrome or color camera  e.g. IS_COLORMODE_BAYER	*/
//  DWORD         nMaxWidth;          /*!< \brief 	maximum width of the sensor in pixel  e.g. 1280	*/
//  DWORD         nMaxHeight;         /*!< \brief 	maximum width of the sensor in pixel e.g. 1024	*/
//  BOOL          bMasterGain;        /*!< \brief 	does the sensor support using master gain e.g. FALSE	*/
//  BOOL          bRGain;             /*!< \brief 	does the sensor support using gain on red pixels only e.g. TRUE	*/
//  BOOL          bGGain;             /*!< \brief 	does the sensor support using gain on green pixels only e.g. TRUE	*/
//  BOOL          bBGain;             /*!< \brief 	does the sensor support using gain on blue pixels only e.g. TRUE	*/
//  BOOL          bGlobShutter;       /*!< \brief 	does the sensor support global shutter mode e.g. TRUE	*/
//  char			Reserved[16];		/*!< \brief 	reserved for future use */
//} SENSORINFO, *PSENSORINFO;
//#   endif


//#   ifdef _SENSORSCALERINFO
//#       undef _SENSORSCALERINFO
//#   endif
//#   ifdef SENSORSCALERINFO
//#       undef SENSORSCALERINFO
//#   endif
//typedef struct _SENSORSCALERINFO
//  {
//      INT       nCurrMode;
//      INT       nNumberOfSteps;
//      double    dblFactorIncrement;
//      double    dblMinFactor;
//      double    dblMaxFactor;
//      double    dblCurrFactor;
//      INT       nSupportedModes;
//      BYTE      bReserved[84];
//  } SENSORSCALERINFO;

/* Old defines for flash */
#define IS_GET_FLASHSTROBE_MODE             0x8000
#define IS_GET_FLASHSTROBE_LINE             0x8001
#define IS_GET_SUPPORTED_FLASH_IO_PORTS     0x8002

#define IS_SET_FLASH_OFF                    0
#define IS_SET_FLASH_ON                     1
#define IS_SET_FLASH_LO_ACTIVE              IS_SET_FLASH_ON
#define IS_SET_FLASH_HI_ACTIVE              2
#define IS_SET_FLASH_HIGH                   3
#define IS_SET_FLASH_LOW                    4
#define IS_SET_FLASH_LO_ACTIVE_FREERUN      5
#define IS_SET_FLASH_HI_ACTIVE_FREERUN      6
#define IS_SET_FLASH_IO_1                   0x0010
#define IS_SET_FLASH_IO_2                   0x0020
#define IS_SET_FLASH_IO_3                   0x0040
#define IS_SET_FLASH_IO_4                   0x0080
#define IS_FLASH_IO_PORT_MASK               (IS_SET_FLASH_IO_1 | IS_SET_FLASH_IO_2 | IS_SET_FLASH_IO_3 | IS_SET_FLASH_IO_4)  

#define IS_GET_FLASH_DELAY                  -1
#define IS_GET_FLASH_DURATION               -2
#define IS_GET_MAX_FLASH_DELAY              -3
#define IS_GET_MAX_FLASH_DURATION           -4
#define IS_GET_MIN_FLASH_DELAY              -5
#define IS_GET_MIN_FLASH_DURATION           -6
#define IS_GET_FLASH_DELAY_GRANULARITY      -7
#define IS_GET_FLASH_DURATION_GRANULARITY   -8

#endif  // DS_EXPORT

// ============================================================================
/*! \defgroup IuEyeCapture uEye Capture Interface
 *  Proprietary interfaces for extra functionality exposed by the capture filter
 *  It adds functions for hot pixel and whitebalance user control as well as
 *  parameter persistence.
 * \{
 */
// ============================================================================

// {7BDFA675-E6BF-449e-8349-5F62AE9E0023}
DEFINE_GUID(IID_IuEyeCapture, 
            0x7bdfa675, 0xe6bf, 0x449e, 0x83, 0x49, 0x5f, 0x62, 0xae, 0x9e, 0x0, 0x23);

interface IuEyeCapture : public IUnknown
{
    /*!
	*   \brief 	Returns hardware gain factors in percent
    *	\param 	plRed       Receives the red gain factor
    *	\param 	plGreen     Receives the green gain factor
    *	\param 	plBlue      Receives the blue gain factor
    *	\return	HRESULT     0 on success, error code otherwise.
    */
    STDMETHOD(GetWhiteBalanceMultipliers)(long *plRed, long *plGreen, long *plBlue) = 0;

    /*!
	*   \brief 	Sets hardware gain factors
    *	\param 	lRed        red gain factor to set in percent (457 means a factor of 4.57)
    *	\param 	lGreen      green gain factor to set in percent
    *	\param 	lBlue       blue gain factor to set in percent
    *	\return	HRESULT     0 on success, error code otherwise.
    */
    STDMETHOD(SetWhiteBalanceMultipliers)(long lRed, long lGreen, long lBlue) = 0;

    /*!
	*	\brief	OBSOLETE: Queries the number of connected camera devices.
	*/
	STDMETHOD(GetNumberOfCameras)(long *plNr) = 0;

    /*!
	*	\brief 	Returns the device info for the connected camera as a pair of CAMERAINFO and SENSORINFO
    *	\param 	psInfo      Receives the SENSORINFO
    *	\param 	pcInfo      Receives the CAMERAINFO
    *	\return	HRESULT     0 on success, error code otherwise.
    */
	STDMETHOD(GetDeviceInfo)(SENSORINFO *psInfo, CAMERAINFO *pcInfo) = 0;

	/*!
	*	\brief	Queries the Version of the installed uEye Driver files
     *	\param 	pVersion    Receives the Version of connected cameras.
     *	\return	HRESULT     0 on success, error code otherwise.
     *
     *  \note   This is not the Version of the uEye capture device filter but
     *          the Version of the underlying driver files.
	 */
	STDMETHOD(GetDLLVersion)(long *pVersion) = 0;

	/*!
	*	\brief 	OBSOLETE: Returns a pair of lists, containing of CAMERAINFO and SENSORINFO structures, which holds
	*		information of the available cameras.
	*/ 
	STDMETHOD(GetListOfCameras)(CAMERAINFO **cInfo, SENSORINFO **sInfo, long *lNr) = 0;

	/*!
	*	\brief 	OBSOLETE: Tries to connect the filter to another camera.
	*/
	STDMETHOD(ConnectToCamera)(long lIndex) = 0;

	/*!
	*	\brief	Activates or deactivates the hot pixel correction.
     *	\param 	lEnable     Set to 1 to activate or 0 to deactivate correction.
     *	\return	HRESULT     0 on success, error code otherwise.
	 *	\see    GetBadPixelCorrection
	 */
	STDMETHOD(SetBadPixelCorrection)(long lEnable) = 0;

	/*!	 
	*	\brief	Queries the current state of the hot pixel correction unit.
     *	\param 	plEnable    Receives 1 if hot pixel correction is enabled.
     *	\return	HRESULT     0 on success, error code otherwise.
	 *	\see    SetBadPixelCorrection
	 */
	STDMETHOD(GetBadPixelCorrection)(long *plEnable) = 0;

	/*!	 
	*	\brief	Configures the hot pixel correction of the sensor
     *	\param 	nMode          Selection Mode of Hotpixel
	 *          pParam	       Pointer to function parameter, depends
	 *					       on the selection of nMode
	 *          SizeOfParams   Size of function parameter memory in bytes
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(HotPixel)(UINT nMode, void *pParam, UINT SizeOfParam) = 0;

    /*!	 
	* \brief	Loads previous stored camera settings.
    *	\return	HRESULT     0 on success, error code otherwise.
    *	\see    SaveSettings
    */
    STDMETHOD(LoadSettings)(void) = 0;
    
    /*!	 
	* \brief	Stores the current set camera settings in the registry.
    *	\return	HRESULT     0 on success, error code otherwise.
    *	\see    LoadSettings
    *
    *   \note Data will be stored individual for each uEye UI model (e.g. UI1410-C).
    */
	STDMETHOD(SaveSettings)(void) = 0;

	/*!
	* \brief	Resets the camera parameters to the driver defaults.
     *	\return	HRESULT     0 on success, error code otherwise.
     *	\see     LoadParameters, SaveParameters
     *
     *  \note   You may not be able to reset parameters while the filter is 
     *          connected or running.
	 */
    STDMETHOD(ResetDefaults)(void) = 0;
};
/*!
 * \}
 */	// end of group uEyeCapture Interface


// {E179D0EE-E0BB-42d6-BAB9-FFDF2277E887}
DEFINE_GUID(IID_IuEyeCaptureEx, 
            0xe179d0ee, 0xe0bb, 0x42d6, 0xba, 0xb9, 0xff, 0xdf, 0x22, 0x77, 0xe8, 0x87);

interface IuEyeCaptureEx : public IuEyeCapture
{
 	
	/*!
	* \brief	Activates or deactivates the additional gain amplification
    *  \param  lGainBoost  Set to 1 to activate or 0 to deactivate extra amplification.
    *  \return HRESULT     0 on success, error code otherwise.
    *  \see    GetGainBoost
    */
    STDMETHOD(SetGainBoost)(long lGainBoost) = 0;

    /*!
	* \brief	Queries the current state of the extra amplification.
    *  \param  plGainBoost Receives 1 if extra amplification is enabled.
    *  \return HRESULT     0 on success, error code otherwise.
    *  \see    SetGainBoost
    */
    STDMETHOD(GetGainBoost)(long *plGainBoost) = 0;

    /*!
	* \brief	Activates or deactivates the hardware gamma.
    *  \param  lHWGamma    Set to 1 to activate or 0 to deactivate hw gamma.
    *  \return HRESULT     0 on success, error code otherwise.
    *  \see    GetHardwareGamma
    */
    STDMETHOD(SetHardwareGamma)(long lHWGamma) = 0;

    /*!
	* \brief	Queries the current state of hardware gamma.
    *  \param  plHWGamma    Receives 1 if hw gamma is enabled.
    *  \return HRESULT      0 on success, error code otherwise.
    *  \see    SetHardwareGamma
    */
    STDMETHOD(GetHardwareGamma)(long *plHWGamma) = 0;

	/*!	 
	 * \brief	Load/save the parameters from/to a file or camera EEPRom userset.
     *	\param 	nMode          Selection Mode of ParameterSet
	 *          pParam	       Pointer to function parameter, depends
	 *					       on the selection of nMode
	 *          SizeOfParams   Size of function parameter memory in bytes
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(ParametersSet)(UINT nMode, void *pParam, UINT SizeOfParam) = 0;

    /*!
	* \brief	Load the parameters from a file or camera EEPRom userset.
    *	\param 	cszFileName Filename or EEPRom userset to load parameters from.
    *	\return	HRESULT     0 on success, error code otherwise.
    *	\see    SaveParameters, ResetDefaults
    *
    *  \note   You may not be able to load parameters while the filter is 
    *          connected or running.
    */
    STDMETHOD(LoadParameters)(const char* cszFileName) = 0;

    /*!
	* \brief	Stores the current parameters to file or camera EEPRom userset.
    *	\param 	cszFileName Filename or EEPRom userset to store parameters to.
    *	\return	HRESULT     0 on success, error code otherwise.
    *	\see    LoadParameters, ResetDefaults
    */
    STDMETHOD(SaveParameters)(const char* cszFileName) = 0;
};
/*!
* \}
*/	// end of group uEyeCaptureInterface


// ============================================================================
/*! \defgroup IuEyeAutoFeatures uEye Auto Feature Interface
*  Proprietary interface for uEye auto feature control exposed by the capture
*  filter. Allows a DirectShow based program to control and query all auto
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {666A7ED1-C64F-47e8-A8D2-E381FD353315}
DEFINE_GUID(IID_IuEyeAutoFeatures, 
            0x666a7ed1, 0xc64f, 0x47e8, 0xa8, 0xd2, 0xe3, 0x81, 0xfd, 0x35, 0x33, 0x15);

interface IuEyeAutoFeatures : public IUnknown
{
	/*!
	* \brief	Specifies the brightness reference value which should be achieved by auto gain and auto exposure.
     *  \param  lReference  The reference value the controller should reach.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    GetAutoBrightnessReference, SetAutoBrightnessMaxExposure, SetAutoBrightnessMaxGain
     */
    STDMETHOD(SetAutoBrightnessReference)(long lReference) = 0;

    /*!
	* \brief 	Queries the actual set reference value for auto brightness control.
     *  \param  plReference Receives the current value for reference.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoBrightnessReference
     */
    STDMETHOD(GetAutoBrightnessReference)(long* plReference) = 0;

    /*!
	* \brief 	Upper limit of the exposure time when used to control the image brightness automatically.  
     *  \param  lMaxExposure Maximum exposure time (in us Units) the controller is allowed to set.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see GetAutoBrightnessMaxExposure, SetAutoBrightnessMaxGain, SetAutoBrightnessReference
     */
    STDMETHOD(SetAutoBrightnessMaxExposure)(long lMaxExposure) = 0;

    /*!
	* \brief 	Queries the actual set upper limit of automatic controlled exposure time.
     *  \param  plMaxExposure Receives the currently allowed maximum exposure time (us Units)
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoBrightnessMaxExposure
     */
    STDMETHOD(GetAutoBrightnessMaxExposure)(long* plMaxExposure) = 0;

    /*!
	* \brief 	Upper limit of gain when used to control the image brightness automatically.
     *  \param  lMaxGain    Maximum master gain value the controller is allowed to set.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoBrightnessMaxExposure, GetAutoBrightnessMaxGain, SetAutoBrightnessReference
     */
    STDMETHOD(SetAutoBrightnessMaxGain)(long lMaxGain) = 0;

    /*!
	* \brief 	Queries the actual set upper limit of automatic controlled master gain amplifier. 
     *  \param  plMaxGain   Receives the currently allowed maximum gain value.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoBrightnessMaxGain
     */
    STDMETHOD(GetAutoBrightnessMaxGain)(long* plMaxGain) = 0;

    /*!
	* \brief 	Controls the percentage of examined images for the automatic brightness control unit.
     *  \param  lSpeed      The desired speed in a range of 1%(slow) to 100%(fast).
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    GetAutoBrightnessSpeed, SetAutoBrightnessReference
     */
    STDMETHOD(SetAutoBrightnessSpeed)(long lSpeed) = 0;

    /*!
	* \brief 	Queries the actual set rate at which image brightness is examined. 
     *  \param  plSpeed     Receives the currently set examination speed.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoBrightnessSpeed
     */
    STDMETHOD(GetAutoBrightnessSpeed)(long* plSpeed) = 0;

    /*!
	* \brief 	Specifies the area of interest within the image in which the brightness should be examined.
     *  \param  lXPos       Left bound of the area of interest.
     *  \param  lYPos       Upper bound of the area of interest.
     *  \param  lWidth      Width of the area of interest.
     *  \param  lHeight     Height of the area of interest.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    GetAutoBrightnessAOI
     */
    STDMETHOD(SetAutoBrightnessAOI)(long lXPos, long lYPos, long lWidth, long lHeight) = 0;

    /*!
	* \brief 	Queries the actual used area of interest in which the brightness is examined.
     *  \param  plXPos      Receives the left bound.
     *  \param  plYPos      Receives the upper bound.
     *  \param  plWidth     Receives the width.
     *  \param  plHeight    Receives the height.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoBrightnessAOI
     */
    STDMETHOD(GetAutoBrightnessAOI)(long* plXPos, long* plYPos, long* plWidth, long* plHeight) = 0;

    /*!
	* \brief 	Specifies relative offsets between the individual color channels when used by the automatic whitebalance control unit.
     *  \param  lRedOffset  Offset for the red gain channel relative to the green one.
     *  \param  lBlueOffset Offset for the blue gain channel relative to the green one.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    GetAutoWBGainOffsets, SetAutoWBGainRange
     */
    STDMETHOD(SetAutoWBGainOffsets)(long lRedOffset, long lBlueOffset) = 0;

    /*!
	* \brief 	Queries the actual set color channel offsets for automatic whitebalance.
     *  \param  plRedOffset  Receives the red to green channel offset.
     *  \param  plBlueOffset  Receives the blue to green channel offset.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoWBGainOffsets
     */
    STDMETHOD(GetAutoWBGainOffsets)(long* plRedOffset, long* plBlueOffset) = 0;

    /*!
	* \brief 	Limits the range the automatic whitebalance controller unit is allowed to use when adjusting the RGB gain channels.
     *  \param  lMinRGBGain  Minimum allowed gain value.
     *  \param  lMaxRGBGain  Maximum allowed gain value.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    GetAutoWBGainRange, SetAutoWBGainOffsets
     */
    STDMETHOD(SetAutoWBGainRange)(long lMinRGBGain, long lMaxRGBGain) = 0;

    /*!
	* \brief 	Queries the actual allowed gain range for the automatic whitebalance controller unit.
     *  \param  plMinRGBGain  Receives the currently allowed minimal gain value.
     *  \param  plMaxRGBGain  Receives the currently allowed maximal gain value.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoWBGainRange
     */
    STDMETHOD(GetAutoWBGainRange)(long* plMinRGBGain, long* plMaxRGBGain) = 0;

    /*!
	* \brief	Controls the percentage of examined images for the automatic whitebalance control unit.
     *  \param  lSpeed      The desired speed in a range of 1%(slow) to 100%(fast).
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoWBGainRange, SetAutoWBGainOffsets
     */
    STDMETHOD(SetAutoWBSpeed)(long lSpeed) = 0;

    /*!
	* \brief 	Queries the actual set rate at which the images whitebalance is examined. 
     *  \param  plSpeed     Receives the currently set examination speed.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoWBSpeed
     */
    STDMETHOD(GetAutoWBSpeed)(long* plSpeed) = 0;

    /*!
	* \brief 	Specifies the area of interest within the image in which the whitebalance should be examined.
     *  \param  lXPos       Left bound of the area of interest.
     *  \param  lYPos       Upper bound of the area of interest.
     *  \param  lWidth      Width of the area of interest.
     *  \param  lHeight     Height of the area of interest.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    GetAutoWBAOI
     */
    STDMETHOD(SetAutoWBAOI)(long lXPos, long lYPos, long lWidth, long lHeight) = 0;

    /*!
	* \brief 	Queries the actual used area of interest in which the whitebalance is examined.
     *  \param  plXPos      Receives the left bound.
     *  \param  plYPos      Receives the upper bound.
     *  \param  plWidth     Receives the width.
     *  \param  plHeight    Receives the height.
     *  \return HRESULT     0 on success, error code otherwise.
     *  \see    SetAutoWBAOI
     */
    STDMETHOD(GetAutoWBAOI)(long* plXPos, long* plYPos, long* plWidth, long* plHeight) = 0;
};
/*!
 * \}
 */	// end of group IuEyeAutoFeatures


// E122A994-FC4D-445b-B21C-308B674844E0
DEFINE_GUID(IID_IuEyeFaceDetection, 
            0xe122a994, 0xfc4d, 0x445b, 0xb2, 0x1c, 0x30, 0x8b, 0x67, 0x48, 0x44, 0xe0);

#ifndef DS_EXPORT
#   define DS_EXPORT
#   ifdef _UEYETIME
#       undef _UEYETIME
#   endif
#   ifdef UEYETIME
#       undef UEYETIME
#   endif
/*!
 * \brief uEye time data type.
 * Used in \see FDT_INFO_EL.
 */
typedef struct _UEYETIME
{
    WORD      wYear;
    WORD      wMonth;
    WORD      wDay;
    WORD      wHour;
    WORD      wMinute;
    WORD      wSecond;
    WORD      wMilliseconds;
    BYTE      byReserved[10];
} UEYETIME;
#endif  /* DS_EXPORT */

#ifndef DS_EXPORT
#   define DS_EXPORT
#   ifdef S_FDT_INFO_EL
#       undef S_FDT_INFO_EL
#   endif
#   ifdef FDT_INFO_EL
#       undef FDT_INFO_EL
#   endif
/*!
 * \brief uEye face detection info element data type.
 * Info on a single detected face as listed by \see FDT_INFO_LIST.
 */
typedef struct S_FDT_INFO_EL
{
    INT nFacePosX;              /*!< \brief	Start X position.                                                           	*/
    INT nFacePosY;              /*!< \brief	Start Y position.                                                           	*/
    INT nFaceWidth;             /*!< \brief	Face width.                                                                 	*/
    INT nFaceHeight;            /*!< \brief	Face height.                                                                	*/
    INT nAngle;                 /*!< \brief	Face Angle (0...360° clockwise, 0° at twelve o'clock position. -1: undefined ).  */
    UINT nPosture;              /*!< \brief	Face posture.                                                               	*/
    UEYETIME TimestampSystem;   /*!< \brief	System time stamp (device query time) .                                     	*/
    UINT64 nReserved;           /*!< \brief	Reserved for future use.                                                    	*/
    UINT nReserved2[4];         /*!< \brief	Reserved for future use.                                                    	*/
} FDT_INFO_EL;
#endif  /* DS_EXPORT */

#ifndef DS_EXPORT
#   define DS_EXPORT
#   ifdef S_FDT_INFO_LIST
#       undef S_FDT_INFO_LIST
#   endif
#   ifdef FDT_INFO_LIST
#       undef FDT_INFO_LIST
#   endif
/*!
 * \brief uEye face detection info list data type.
 * List of detected faces, lists \see FDT_INFO_EL objects.
 */
typedef struct S_FDT_INFO_LIST
{
    UINT nSizeOfListEntry;      /*!< \brief	Size of one list entry in byte(in).  	*/
    UINT nNumDetectedFaces;     /*!< \brief	Number of detected faces(out).       	*/
    UINT nNumListElements;      /*!< \brief	Number of list elements(in).         	*/ 
    UINT nReserved[4];          /*!< \brief	reserved for future use(out).       	*/ 
    FDT_INFO_EL FaceEntry[1];   /*!< \brief	First face entry.                    		*/
} FDT_INFO_LIST;
#endif  /* DS_EXPORT */

// ============================================================================
/*! \defgroup IuEyeFaceDetection uEye Face Detection Interface
*  Proprietary interface for uEye face detection control exposed by the capture
*  filter. Allows a DirectShow based program to control and query the face detection
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================
interface IuEyeFaceDetection : public IUnknown
{
    /*!
     * \brief Query for support of the face detection feature.
     * \param pbSupported		output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of the face detection feature.
     * \param pbEnabled		output location for result.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_IsEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of the face detection feature.
     * \param bEnable		new 'enabled' status.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_Enable)(bool bEnable) = 0;

    /*!
     * \brief Query the current 'suspended' status of the face detection feature.
     * \param pbSuspended   output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_IsSuspended)(bool* pbSuspended) = 0;

    /*!
     * \brief Set the 'suspended' status of the face detection feature.
     * \param bSuspend		new 'suspended' status.
	 * \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_Suspend)(bool bSuspend) = 0;

    /*!
     * \brief Query the current enabled status of the face detection's 'search angle' subfeature.
     * \param pbEnabled		output location for result.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_IsSearchAngleEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of the face detection's 'search angle' feature.
	 * \param	bEnable  	new 'search angle enable' status.
	 * \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_EnableSearchAngle)(bool bEnable) = 0;

    /*!
     * \brief Query the current search angle.
     * \param pulAngle		output location for result.
	 * \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_GetSearchAngle)(long* pulAngle) = 0;

    /*!
     * \brief Set the new search angle.
     * \param ulAngle		output location for result.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_SetSearchAngle)(long ulAngle) = 0;

    /*!
     * \brief Query the last determined face list.
     * \param pFaceList		output location for result: preallocated object of type \see FDT_INFO_LIST.
     * \param ulSize		size of pFaceList in bytes.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_GetFaceList)(void* pFaceList, unsigned long ulSize) = 0;

    /*!
     * \brief Query the last determined number of faces.
     * \param pulNumFaces   output location for result.
	 * \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_GetNumFaces)(unsigned long* pulNumFaces) = 0;

    /*!
     * \brief Query the maximum number of faces that the feature can detect in an image.
     * \param pulMaxNumFaces    output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_GetMaxNumFaces)(unsigned long* pulMaxNumFaces) = 0;

    /*!
     * \brief Query the current maximum number of overlay drawings that the feature will show in an image.
     * \param pulMaxNumOvl		output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_GetMaxNumOvl)(unsigned long* pulMaxNumOvl) = 0;

    /*!
     * \brief Set the new maximum number of overlay drawings that the feature will show in an image.
     * \param ulMaxNumOvl		the new setting.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_SetMaxNumOvl)(unsigned long ulMaxNumOvl) = 0;

    /*!
     * \brief Query the current linewidth for the overlay drawings.
     * \param pulLineWidthOvl   output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_GetLineWidthOvl)(unsigned long* pulLineWidthOvl) = 0;

    /*!
     * \brief Set the new linewidth for the overlay drawings.
     * \param ulLineWidthOvl    the new setting.
	 * \return	HRESULT			S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_SetLineWidthOvl)(unsigned long ulLineWidthOvl) = 0;

    /*!
     * \brief Query the resolution.
     * \param pulHorzRes    output location for result horizontal resolution.
     * \param pulVertRes    output location for result vertical resolution.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(FDT_GetResolution)(unsigned long* pulHorzRes, unsigned long* pulVertRes) = 0;

    /*!
     * \brief Generic access to the face detection feature.
     * \return E_NOTIMPL
     * \note the generic access interface is provided for future use.
     */
    STDMETHOD(FDT_GenericAccess)(unsigned long ulCommand, void* pParam, unsigned long ulSize) = 0;
};
/*!
 * \}
 */	// end of group IuEyeFaceDetection


// ============================================================================
/*! \defgroup IuEyeImageStabilization uEye Image Stabilization Interface
*  Proprietary interface for uEye image stabilization control exposed by the capture
*  filter. Allows a DirectShow based program to control and query the image stabilization
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {D24BA219-7426-45b9-960A-126246ED0897}
DEFINE_GUID(IID_IuEyeImageStabilization, 
            0xd24ba219, 0x7426, 0x45b9, 0x96, 0xa, 0x12, 0x62, 0x46, 0xed, 0x8, 0x97);

interface IuEyeImageStabilization : public IUnknown
{
    /*!
     * \brief Query for support of the image stabilization feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(ImgStab_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of the image stabilization feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(ImgStab_IsEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of the image stabilization feature.
     * \param bEnable   new 'enabled' status.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(ImgStab_Enable)(bool bEnable) = 0;

    /*!
     * \brief Generic access to the image stabilization feature.
     * \return E_NOTIMPL
     * \note the generic access interface is provided for future use.
     */
    STDMETHOD(ImgStab_GenericAccess)(unsigned long ulCommand, void* pParam, unsigned long ulSize) = 0;
};
/*!
 * \}
 */	// end of group IuEyeImageStabilization

// ============================================================================
/*! \defgroup IuEyeSensorAWB uEye Sensor Auto White Balance Interface
*  Proprietary interface for uEye auto white balance feature given by sensor exposed by 
*  the capture filter. Allows a DirectShow based program to control and query the sensor's
*  auto white  balance feature related parameters that are not accessible via direct 
*  show functions.
* \{
*/
// ============================================================================

// {E737FA4C-2160-45a5-95D3-CE6B069D9AB3}
DEFINE_GUID(IID_IuEyeSensorAWB, 
            0xe737fa4c, 0x2160, 0x45a5, 0x95, 0xd3, 0xce, 0x6b, 0x6, 0x9d, 0x9a, 0xb3);

interface IuEyeSensorAWB : public IUnknown
{
    /*!
     * \brief Query for support of sensor's awb feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(SensorAWB_IsSupported)(bool* pbSupported) = 0;

    /*!
	* \brief 	Query the current 'enabled' status of sensor's awb feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(SensorAWB_IsEnabled)(bool* pbEnabled) = 0;

    /*!
	* \brief 	Set the 'enabled' status of sensor's awb feature.
     * \param bEnable   new 'enabled' status.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(SensorAWB_Enable)(bool bEnable) = 0;

    /*!
	* \brief 	Query the current mode of sensor's awb feature.
     * \param pulMode   current mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(SensorAWB_GetMode)(unsigned long* pulMode) = 0;

    /*!
	* \brief 	Set the mode of sensor's awb feature.
     * \param ulMode    mode to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(SensorAWB_SetMode)(unsigned long ulMode) = 0;

    /*!
	* \brief 	Query the supported modes of sensor's awb feature.
     * \param pulModes  bitmask containing supported modes.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(SensorAWB_GetSupportedModes)(unsigned long* pulModes) = 0;
};
/*!
 * \}
 */	// end of group IuEyeSensorAWB

// ============================================================================
/*! \defgroup IuEyeAutoContrast uEye Auto Contrast Correction Interface
*  Proprietary interface for uEye auto contrast correction feature exposed by 
*  the capture filter. Allows a DirectShow based program to control and query the sensor's
*  auto contrast feature related parameters that are not accessible via direct 
*  show functions.
*  \note auto contrast correction can not be used if auto backlight compensation is
*  enabled and vice versa.
* \{
*/
// ============================================================================

// {CC2FCD9E-478A-42d9-9832-A3CC29D05098}
DEFINE_GUID(IID_IuEyeAutoContrast, 
            0xcc2fcd9e, 0x478a, 0x42d9, 0x98, 0x32, 0xa3, 0xcc, 0x29, 0xd0, 0x50, 0x98);

interface IuEyeAutoContrast : public IUnknown
{
    /*!
     * \brief Query for support of auto contrast correction feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoContrast_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of auto contrast correction feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoContrast_IsEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Query the current value of auto contrast correction feature.
     * \param pdblCorrValue   current correction value.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoContrast_GetValue)(double* pdblCorrValue) = 0;

    /*!
     * \brief Set the value of auto contrast correction feature.
     * \param dblCorrValue   value to set.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoContrast_SetValue)(double dblCorrValue) = 0;

    /*!
     * \brief Query the default value of auto contrast correction feature.
     * \param pdblCorrValue   default value.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoContrast_GetDefaultValue)(double* pdblCorrValue) = 0;

    /*!
     * \brief Query the range of auto contrast correction feature.
     * \param pdblMin   minimum value.
     * \param pdblMax   maximum value.
     * \param pdblInc   step width.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoContrast_GetRange)(double* pdblMin, double* pdblMax, double* pdblInc) = 0;
};
/*!
 * \}
 */	// end of group IuEyeAutoContrast

// ============================================================================
/*! \defgroup IuEyeAutoBacklight uEye Auto Backlight Compensation Interface
*  Proprietary interface for uEye auto backlight compensation feature exposed by 
*  the capture filter. Allows a DirectShow based program to control and query the sensor's
*  auto backlight compensation feature related parameters that are not accessible via direct 
*  show functions.
*  \note auto backlight compensation can not be used if auto contrast correction is
*  enabled and vice versa.
* \{
*/
// ============================================================================

// {A7CBC666-1A97-4af9-9652-4E34835F77CD}
DEFINE_GUID(IID_IuEyeAutoBacklight, 
            0xa7cbc666, 0x1a97, 0x4af9, 0x96, 0x52, 0x4e, 0x34, 0x83, 0x5f, 0x77, 0xcd);

interface IuEyeAutoBacklight : public IUnknown
{
    /*!
     * \brief Query for support of sensor's auto backlight compensation feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoBacklight_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of sensor's auto backlight compensation feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoBacklight_IsEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of sensor's auto backlight compensation feature.
     * \param bEnable   new 'enabled' status.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoBacklight_Enable)(bool bEnable) = 0;

    /*!
     * \brief Query the current mode of sensor's auto backlight compensation feature.
     * \param pulMode   current mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoBacklight_GetMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Set the mode of sensor's auto backlight compensation feature.
     * \param ulMode    mode to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoBacklight_SetMode)(unsigned long ulMode) = 0;

    /*!
     * \brief Query the default mode of sensor's auto backlight compensation feature.
     * \param pulMode   default mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoBacklight_GetDefaultMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Query the supported modes of sensor's auto backlight compensation feature.
     * \param pulModes  bitmask containing supported modes.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoBacklight_GetSupportedModes)(unsigned long* pulModes) = 0;
};
/*!
 * \}
 */	// end of group IuEyeAutoBacklight

// ============================================================================
/*! \defgroup IuEyeAntiFlicker uEye Anti Flicker Interface
*  Proprietary interface for uEye anti flicker feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the sensor's anti flicker
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================


// {6B2A1AB6-E324-4d86-9637-2E783F50497A}
DEFINE_GUID(IID_IuEyeAntiFlicker, 
            0x6b2a1ab6, 0xe324, 0x4d86, 0x96, 0x37, 0x2e, 0x78, 0x3f, 0x50, 0x49, 0x7a);

interface IuEyeAntiFlicker : public IUnknown
{
    /*!
     * \brief Query for support of sensor's anti flicker feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(AntiFlicker_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current mode of sensor's anti flicker feature.
     * \param pulMode   current mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AntiFlicker_GetMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Set the mode of sensor's anti flicker feature.
     * \param ulMode    mode to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AntiFlicker_SetMode)(unsigned long ulMode) = 0;

    /*!
     * \brief Query the default mode of sensor's anti flicker feature.
     * \param pulMode   default mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AntiFlicker_GetDefaultMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Query the supported modes of sensor's anti flicker feature.
     * \param pulModes  bitmask containing supported modes.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(AntiFlicker_GetSupportedModes)(unsigned long* pulModes) = 0;
};
/*!
 * \}
 */	// end of group IuEyeAntiFlicker

// ============================================================================
/*! \defgroup IuEyeScenePreset uEye Scene Preset Interface
*  Proprietary interface for uEye scene preset feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the sensor's scene preset
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {E83A0636-194B-4ad8-BBD2-CD91AE35F136}
DEFINE_GUID(IID_IuEyeScenePreset, 
            0xe83a0636, 0x194b, 0x4ad8, 0xbb, 0xd2, 0xcd, 0x91, 0xae, 0x35, 0xf1, 0x36);


interface IuEyeScenePreset : public IUnknown
{
    /*!
     * \brief Query for support of sensor's scene preset feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(ScenePreset_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current mode of sensor's scene preset feature.
     * \param pulMode   current mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(ScenePreset_GetMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Set the mode of sensor's scene preset feature.
     * \param ulMode    mode to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(ScenePreset_SetMode)(unsigned long ulMode) = 0;

    /*!
     * \brief Query the default mode of sensor's scene preset feature.
     * \param pulMode   default mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(ScenePreset_GetDefaultMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Query the supported modes of sensor's scene preset feature.
     * \param pulModes  bitmask containing supported modes.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(ScenePreset_GetSupportedModes)(unsigned long* pulModes) = 0;
};
/*!
 * \}
 */	// end of group IuEyeScenePreset

// ============================================================================
/*! \defgroup IuEyeDigitalZoom uEye Digital Zoom Interface
*  Proprietary interface for uEye digital zoom feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the digital zoom
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {25F131C3-8F93-484f-8B4A-474313EEDDF5}
DEFINE_GUID(IID_IuEyeDigitalZoom, 
            0x25f131c3, 0x8f93, 0x484f, 0x8b, 0x4a, 0x47, 0x43, 0x13, 0xee, 0xdd, 0xf5);


interface IuEyeDigitalZoom : public IUnknown
{
    /*!
     * \brief Query for support of digital zoom feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(DigitalZoom_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the number of supported zoom factors used by digital zoom feature.
     * \param pulNumFactors number of supported factors
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(DigitalZoom_GetNumZoomFactors)(unsigned long* pulNumFactors) = 0;

    /*!
     * \brief Query the supported zoom factors of digital zoom feature.
     * \param pZFList   output location for result: preallocated object of type double.
     * \param ulSize    size of pZFList in bytes.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(DigitalZoom_GetZoomFactors)(void* pZFList, unsigned long ulSize) = 0;

    /*!
     * \brief Query the current zoom factor of digital zoom feature.
     * \param pdblZoomFactor current zoom factor.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(DigitalZoom_GetZoomFactor)(double* pdblZoomFactor) = 0;

    /*!
     * \brief Set the zoom factor of digital zoom feature.
     * \param dblZoomFactor zoom factor to set.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(DigitalZoom_SetZoomFactor)(double dblZoomFactor) = 0;

    /*!
     * \brief Query the default zoom factors of digital zoom feature.
     * \param pdblDefault   Default zoom values.
     * \return HRESULT:		S_OK on success, error code otherwise.
     */
	STDMETHOD(DigitalZoom_GetZoomFactorDefault)(double* pdblDefault) = 0;

    /*!
     * \brief Query the zoom factor range of digital zoom feature.
     * \param pdblMin   minimum zoom factor.
     * \param pdblMax   maximum zoom factor.
     * \param pdblInc   increment of zoom factor.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
	STDMETHOD(DigitalZoom_GetZoomFactorRange)(double* pdblMin, double* pdblMax, double* pdblInc) = 0;
};
/*!
 * \}
 */	// end of group IuEyeDigitalZoom


// ============================================================================
/*! \defgroup IuEyeFocus Focus Interface
*  Proprietary interface for uEye focus feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the digital zoom
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {300F14B1-D486-40fa-9804-9A621C193137}
DEFINE_GUID(IID_IuEyeFocus, 
			0x300f14b1, 0xd486, 0x40fa, 0x98, 0x4, 0x9a, 0x62, 0x1c, 0x19, 0x31, 0x37);

interface IuEyeFocus : public IUnknown
{
    /*!
     * \brief	Query for support of auto focus.
     * \param	pbSupported		output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_IsAutoFocusSupported)(bool* pSupported) = 0;

    /*!
     * \brief	Set the auto focus.
     * \param	bEnalbe		enables or disables the auto focus.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_SetAutoFocus)(bool bEnable) = 0;

    /*!
     * \brief	get the auto focus.
     * \param	pbEnable		output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_GetAutoFocus)(bool* pbEnable) = 0;

    /*!
     * \brief	get the auto focus status.
     * \param	piStatus		output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_GetAutoFocusStatus)(INT* piStatus) = 0;
	
	/*!
     * \brief	Set enable the auto focus once.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_SetEnableAutoFocusOnce)(void) = 0;

    /*!
     * \brief	Set the manual focus.
     * \param   uiManual	Set the manual focus.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_SetManualFocus)(UINT uiManual) = 0;

    /*!
     * \brief	get the manual focus.
     * \param	puiManual		output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_GetManualFocus)(UINT* puiManual) = 0;

	/*!
     * \brief	get the manual focus range.
     * \param	puiMin		Minimum manual focus
     * \param	puiMax		Maximum manual focus
	 * \param	puiInc		Increment of manual focus
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
	STDMETHOD(Focus_GetManualFocusRange)(UINT* puiMin, UINT* puiMax, UINT* puiInc) = 0;

    /*!
     * \brief	Get the current focus zone rect.
     * \param   pfocusZoneRect  variable of type RECT containing the focus zone.
	 * \return	HRESULT			S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZone)(RECT* pfocusZoneRect) = 0;

    /*!
     * \brief	Set the current focus zone rect.
     * \param   focusZoneRect   variable of type RECT containing the focus zone.
	 * \return	HRESULT			S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_SetAutoFocusZone)(RECT focusZoneRect) = 0;

    /*!
     * \brief	Get the default focus zone rect.
     * \param   pfocusZoneRect  variable of type RECT containing the default focus zone.
	 * \return	HRESULT			S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZoneDefault)(RECT* pfocusZoneRectDefault) = 0;

    /*!
     * \brief	Get the minimum focus zone point.
     * \param   pfocusZonePosMin	variable of type POINT containing the minimum focus zone pos.
	 * \return	HRESULT				S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZonePosMin)(POINT* pfocusZonePosMin) = 0;

	/*!
     * \brief	Get the maximum focus zone point.
     * \param   pfocusZonePosMax	variable of type POINT containing the maximum focus zone pos.
	 * \return	HRESULT				S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZonePosMax)(POINT* pfocusZonePosMax) = 0;

	/*!
     * \brief	Get the increment focus zone point.
     * \param   pfocusZonePosInc	variable of type POINT containing the increment focus zone pos.
	 * \return	HRESULT				S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZonePosInc)(POINT* pfocusZonePosInc) = 0;

    /*!
     * \brief	Get the minimum focus zone size.
     * \param   pfocusZoneSizeMin	variable of type SIZE containing the minimum focus zone size.
	 * \return	HRESULT				S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZoneSizeMin)(SIZE* pfocusZoneSizeMin) = 0;

    /*!
     * \brief	Get the maximum focus zone size.
     * \param   pfocusZoneSizeMax	variable of type SIZE containing the maximum focus zone size.
	 * \return	HRESULT				S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZoneSizeMax)(SIZE* pfocusZoneSizeMax) = 0;

	/*!
     * \brief	Get the increment focus zone size.
     * \param   pfocusZoneSizeInc	variable of type SIZE containing the increment focus zone size.
	 * \return	HRESULT				S_OK on success, error code otherwise.
     */
    STDMETHOD(Focus_GetAutoFocusZoneSizeInc)(SIZE* pfocusZoneSizeInc) = 0;

    /*!
     * \brief	Get the auto focus zone weight.
     * \param	iWeightCount		number of zone count
	 * \		piFocusZoneWeight	variable of type INT [] containing the focus zone weights.
	 * \return	HRESULT 			S_OK on success, error code otherwise.
     */
	STDMETHOD (Focus_GetAutoFocusZoneWeight)(INT iWeightCount, INT* piFocusZoneWeight) = 0;

    /*!
     * \brief	Set the auto focus zone weight.
     * \param	iWeightCount		number of zone count
	 * \		piFocusZoneWeight	variable of type INT [] containing the focus zone weights.
	 * \return	HRESULT 			S_OK on success, error code otherwise.
     */
	STDMETHOD (Focus_SetAutoFocusZoneWeight)(INT iWeightCount, INT* piFocusZoneWeight) = 0;

    /*!
     * \brief	Get the auto focus zone weight default.
     * \param	iWeightCount				number of zone count
	 * \		piFocusZoneWeightDefault	variable of type INT [] containing the focus zone weights.
	 * \return	HRESULT 					S_OK on success, error code otherwise.
     */
	STDMETHOD (Focus_GetAutoFocusZoneWeightDefault)(INT iWeightCount, INT* piFocusZoneWeightDefault) = 0;

    /*!
     * \brief	Get the auto focus zone weight count.
     * \param	piWeightCount		get the number of zone count
	 * \return	HRESULT 			S_OK on success, error code otherwise.
     */
	STDMETHOD (Focus_GetAutoFocusZoneWeightCount)(INT* piWeightCount) = 0;

};
/*!
 * \}
 */	// end of group IuEyeFocus


// ============================================================================
/*! \defgroup IuEyeSaturation uEye Saturation Interface
*  Proprietary interface for uEye saturation feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the sensor's saturation
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {EC410EDE-15BC-47b1-9BF7-6CB00F58FF5F}
DEFINE_GUID(IID_IuEyeSaturation, 
            0xec410ede, 0x15bc, 0x47b1, 0x9b, 0xf7, 0x6c, 0xb0, 0xf, 0x58, 0xff, 0x5f);


interface IuEyeSaturation : public IUnknown
{
    /*!
     * \brief Query for support of sensor's saturation feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Saturation_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current saturation value of sensor's saturation feature.
     * \param plValue   current value.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Saturation_GetValue)(long* plValue) = 0;

    /*!
     * \brief Set the saturation value of sensor's saturation feature.
     * \param lValue    mode to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Saturation_SetValue)(long lValue) = 0;

    /*!
     * \brief Query the default value of sensor's saturation feature.
     * \param plDefValue default value.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Saturation_GetDefaultValue)(long* plDefValue) = 0;

    /*!
     * \brief Query the range of sensor's saturation feature.
     * \param plMin     minimum value.
     * \param plMax     maximum value.
     * \param plInc     step width.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Saturation_GetRange)(long* plMin, long* plMax, long* plInc) = 0;
};
/*!
 * \}
 */	// end of group IuEyeSaturation

// ============================================================================
/*! \defgroup IuEyeSharpness uEye Sharpness Interface
*  Proprietary interface for uEye sharpness feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the sensor's sharpness
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {1A30F620-78E0-4061-A730-C7B91848C7D0}
DEFINE_GUID(IID_IuEyeSharpness, 
            0x1a30f620, 0x78e0, 0x4061, 0xa7, 0x30, 0xc7, 0xb9, 0x18, 0x48, 0xc7, 0xd0);

interface IuEyeSharpness : public IUnknown
{
    /*!
     * \brief Query for support of sensor's sharpness feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(Sharpness_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current sharpness value of sensor's sharpness feature.
     * \param plValue   current value.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Sharpness_GetValue)(long* plValue) = 0;

    /*!
     * \brief Set the sharpness value of sensor's sharpness feature.
     * \param lValue    mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Sharpness_SetValue)(long lValue) = 0;

    /*!
     * \brief Query the default value of sensor's sharpness feature.
     * \param plDefValue default value.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Sharpness_GetDefaultValue)(long* plDefValue) = 0;

    /*!
     * \brief Query the range of sensor's sharpness feature.
     * \param plMin     minimum value.
     * \param plMax     maximum value.
     * \param plInc     step width.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Sharpness_GetRange)(long* plMin, long* plMax, long* plInc) = 0;
};
/*!
 * \}
 */	// end of group IuEyeSharpness

// ============================================================================
/*! \defgroup IuEyeColorTemperature uEye Color Temperature Interface
*  Proprietary interface for uEye color temperature feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query thecolor temperatures
*  feature related parameters that are not accessible via direct show functions.
*  \note changing color temperature values is only possible if rgb model is selected.
* \{
*/
// ============================================================================

// {3311AD49-0D54-4016-8FAA-B26CA351311B}
DEFINE_GUID(IID_IuEyeColorTemperature, 
            0x3311ad49, 0xd54, 0x4016, 0x8f, 0xaa, 0xb2, 0x6c, 0xa3, 0x51, 0x31, 0x1b);

interface IuEyeColorTemperature : public IUnknown
{
    /*!
     * \brief Query for support of rgb model feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(RGBModel_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current mode of rgb model feature.
     * \param pulMode   current mode.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(RGBModel_GetMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Set the mode of rgb model feature.
     * \param ulMode    mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(RGBModel_SetMode)(unsigned long ulMode) = 0;

    /*!
     * \brief Query the default mode of rgb model feature.
     * \param pulMode   default mode.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(RGBModel_GetDefaultMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Query the supported modes of rgb model feature.
     * \param pulModes  bitmask containing supported modes.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(RGBModel_GetSupportedModes)(unsigned long* pulModes) = 0;

    /*!
     * \brief Query for support of color temperature feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current color temperature value of color temperature feature.
     * \param pulValue  current value.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_GetValue)(unsigned long* pulValue) = 0;

    /*!
     * \brief Set the color temperature value of color temperature feature.
	* \param	ulValue	mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_SetValue)(unsigned long ulValue) = 0;

    /*!
     * \brief Query the default value of color temperature feature.
     * \param pulDefValue default value.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_GetDefaultValue)(unsigned long* pulDefValue) = 0;

    /*!
     * \brief Query the range of color temperature feature.
     * \param pulMin    minimum value.
     * \param pulMax    maximum value.
     * \param pulInc    step width.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_GetRange)(unsigned long* pulMin, unsigned long* pulMax, unsigned long* pulInc) = 0;

    /*!
     * \brief Set a new lens shading model 
	 * \param ulValue	lens shading model
	 * \return HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_SetLensShadingModel)(UINT ulValue) = 0;

	/*!
     * \brief get a new lens shading model 
	 * \param pulValue	lens shading model
	 * \return HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_GetLensShadingModel)(UINT* pulValue) = 0;

	/*!
     * \brief get lens shading model supported
	 * \param pulValue	lens shading model
	 * \return HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_GetLensShadingModelSupported)(UINT* pulValue) = 0;

	/*!
     * \brief get lens shading model default
	 * \param pulValue	lens shading model
	 * \return HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(ColorTemperature_GetLensShadingModelDefault)(UINT* pulValue) = 0;

};
/*!
 * \}
 */	// end of group IuEyeColorTemperature

// ============================================================================
/*! \defgroup IuEyeTriggerDebounce uEye Trigger Debouncing Interface
*  Proprietary interface for uEye trigger debounce feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the trigger debounces
*  feature related parameters that are not accessible via direct show functions.
*  \note changing trigger debounce values is only possible if any trigger mode is 
*  activated.
*  \note using driver version 3.70 this feature is only supported on GigE cameras.
* \{
*/
// ============================================================================

// {49422CBA-CBD1-48a1-9810-DA3FDDC1FBEA}
DEFINE_GUID(IID_IuEyeTriggerDebounce, 
            0x49422cba, 0xcbd1, 0x48a1, 0x98, 0x10, 0xda, 0x3f, 0xdd, 0xc1, 0xfb, 0xea);

interface IuEyeTriggerDebounce : public IUnknown
{
    /*!
     * \brief Query for support of trigger debounce feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current mode of trigger debounce feature.
     * \param pulMode   current mode.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_GetMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Set the mode of trigger debounce feature.
     * \param ulMode    mode to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_SetMode)(unsigned long ulMode) = 0;

    /*!
     * \brief Query the default mode of trigger debounce feature.
     * \param pulMode   default mode.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_GetDefaultMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Query the supported modes of trigger debounce feature.
     * \param pulModes  bitmask containing supported modes.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_GetSupportedModes)(unsigned long* pulModes) = 0;

    /*!
     * \brief Query the current delay value of trigger debounce feature.
     * \param pulValue  current value.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_GetDelay)(unsigned long* pulValue) = 0;

    /*!
     * \brief Set the delay value of trigger debounce feature.
	* \param	ulValue	delay to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_SetDelay)(unsigned long ulValue) = 0;

    /*!
     * \brief Query the default delay value of trigger debounce feature.
     * \param pulDefValue default value.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_GetDefaultDelay)(unsigned long* pulDefValue) = 0;

    /*!
     * \brief Query the range of trigger debounce feature's delay value.
     * \param pulMin    minimum value.
     * \param pulMax    maximum value.
     * \param pulInc    step width.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(TriggerDebounce_GetDelayRange)(unsigned long* pulMin, unsigned long* pulMax, unsigned long* pulInc) = 0;
};
/*!
 * \}
 */	// end of group IuEyeTriggerDebounce

// ============================================================================
/*! \defgroup IuEyeTrigger uEye Trigger Interface
*  Proprietary interface for additional uEye trigger features exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the additional trigger 
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {00012E12-4696-4eb7-8CB4-DA7E0B782519}
DEFINE_GUID(IID_IuEyeTrigger, 
0x12e12, 0x4696, 0x4eb7, 0x8c, 0xb4, 0xda, 0x7e, 0xb, 0x78, 0x25, 0x19);

interface IuEyeTrigger : public IUnknown
{
    /*!
     * \brief Query if falling edge trigger mode is supported
     * \param pbSupported  output location for result.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(Trigger_IsFallingEdgeSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query if rising edge trigger mode is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Trigger_IsRisingEdgeSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query if software trigger mode is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Trigger_IsSoftwareTriggerSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Set trigger mode to be used
     * \param nMode  trigger mode
     * \return HRESULT: S_OK on success, error code otherwise.
     * \note    only the trigger mode to be used will be set. To
     *          activate triggering use SetMode().
     */
    STDMETHOD(Trigger_SetTriggerMode)(long nMode) = 0;

    /*!
     * \brief Get the current trigger mode
     * \param pnMode  output location for result.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(Trigger_GetTriggerMode)(long* pnMode) = 0;

	/*!
     * \brief Get the current trigger mode
     * \param pnMode  output location for result.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(Trigger_GetTriggerStatus)(long* pnMode) = 0;

    /*!
     * \brief Get burst size supported
     * \param	pnSupported		burst size supported
     * \return	HRESULT:		S_OK on success, error code otherwise.
     */
    STDMETHOD (Trigger_GetBurstSizeSupported)(UINT* pnSupported) = 0;

    /*!
     * \brief Get burst size range
     * \param	pnMin		minimum burst size
	 *			pnMax		maximum burst size
	 *			pnInc		increment burst size
     * \return	HRESULT:	S_OK on success, error code otherwise.
     */
    STDMETHOD (Trigger_GetBurstSizeRange)(UINT* pnMin, UINT* pnMax, UINT* pnInc) = 0;

    /*!
     * \brief Get burst size
     * \param	pnBurstSize		burst size 
     * \return	HRESULT:		S_OK on success, error code otherwise.
     */
    STDMETHOD (Trigger_GetBurstSize)(UINT* pnBurstSize) = 0;

    /*!
     * \brief Set burst size
     * \param	nBurstSize		burst size 
     * \return	HRESULT:		S_OK on success, error code otherwise.
     */
    STDMETHOD (Trigger_SetBurstSize)(UINT nBurstSize) = 0;
};
/*!
 * \}
 */	// end of group IuEyeTrigger


// ============================================================================
/*! \defgroup IuEyeIO uEye IO Interface
*  Proprietary interface for additional uEye IO features exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the additional IO
*  feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {099E7218-2223-415d-89F5-CD3826509BA7}
DEFINE_GUID(IID_IuEyeIO, 
0x99e7218, 0x2223, 0x415d, 0x89, 0xf5, 0xcd, 0x38, 0x26, 0x50, 0x9b, 0xa7);

interface IuEyeIO : public IUnknown
{
	/*!
     * \brief Set the current GPIO
     * \param pnIO  set the GPIO state.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(IO_SetGPIO)(INT nIO) = 0;

	/*!
     * \brief Get the current GPIO
     * \param pnIO  actual GPIO state.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(IO_GetGPIO)(INT* pnIO) = 0;

		/*!
     * \brief Set the current IO Mask
     * \param nIOMask  set the IO Mask.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(IO_SetIOMask)(INT nIOMask) = 0;

	/*!
     * \brief Get the current IO Mask
     * \param pnIOMask  actual IO Mask.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(IO_GetIOMask)(INT* pnIOMask) = 0;

	/*!
     * \brief Get the current IO Mask
     * \param pnIOMaskInSupp  actual IO Mask.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(IO_IOMaskInputSupported)(INT* pnIOMaskInSupp) = 0;

	/*!
     * \brief Get the current IO Mask
     * \param pnIOMaskOutSupp  actual IO Mask.
     * \return HRESULT: S_OK on success, error code otherwise.
     */
    STDMETHOD(IO_IOMaskOutputSupported)(INT* pnIOMaskOutSupp) = 0;
};
/*!
 * \}
 */	// end of group IuEyeIO

// ============================================================================
/*! \defgroup IuEyePhotometry uEye Photometry Interface
*  Proprietary interface for uEye sensor's auto shutter and auto gain feature exposed 
*  by the capture filter. 
*  Allows a DirectShow based program to control and query the sensor's auto shutter
*  and auto gain feature related parameters that are not accessible via direct show functions.
*  \note on uEye XS cameras modes for auto shutter and auto gain have to be set to the same value.
* \{
*/
// ============================================================================

// {5B200824-C3AD-4bcf-B6D7-F4991C7B5BF4}
DEFINE_GUID(IID_IuEyePhotometry, 
            0x5b200824, 0xc3ad, 0x4bcf, 0xb6, 0xd7, 0xf4, 0x99, 0x1c, 0x7b, 0x5b, 0xf4);

interface IuEyePhotometry : public IUnknown
{
    /*!
     * \brief Query for support of sensor's auto gain feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_IsAutoGainSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of sensor's auto gain feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_IsAutoGainEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of sensor's auto gain feature.
     * \param bEnable   new 'enabled' status.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_AutoGainEnable)(bool bEnable) = 0;

    /*!
     * \brief Query the current mode of sensor's auto gain feature.
     * \param pulMode   current mode.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_GetAutoGainMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Set the mode of sensor's auto gain feature.
     * \param ulMode    mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_SetAutoGainMode)(unsigned long ulMode) = 0;

    /*!
     * \brief Query the default mode of sensor's auto gain feature.
     * \param pulMode   default mode.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_GetDefaultAutoGainMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Query the supported modes of sensor's auto gain feature.
     * \param pulModes  bitmask containing supported modes.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_GetSupportedAutoGainModes)(unsigned long* pulModes) = 0;


    /*!
     * \brief Query for support of sensor's auto shutter feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_IsAutoShutterSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of sensor's auto shutter feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_IsAutoShutterEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of sensor's auto shutter feature.
     * \param bEnable   new 'enabled' status.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_AutoShutterEnable)(bool bEnable) = 0;

    /*!
     * \brief Query the current mode of sensor's auto shutter feature.
     * \param pulMode   current mode.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_GetAutoShutterMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Set the mode of sensor's auto shutter feature.
     * \param ulMode    mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_SetAutoShutterMode)(unsigned long ulMode) = 0;

    /*!
     * \brief Query the default mode of sensor's auto shutter feature.
     * \param pulMode   default mode.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_GetDefaultAutoShutterMode)(unsigned long* pulMode) = 0;

    /*!
     * \brief Query the supported modes of sensor's auto shutter feature.
     * \param pulModes  bitmask containing supported modes.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_GetSupportedAutoShutterModes)(unsigned long* pulModes) = 0;

    /*!
     * \brief Query for support of sensor's auto gain shutter feature.
     * \param pbSupported   output location for result.
	 * \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Photometry_IsAutoGainShutterSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of sensor's auto gain shutter feature.
     * \param pbEnabled output location for result.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
	STDMETHOD(Photometry_IsAutoGainShutterEnabled)(bool* pbEnabled) = 0;
};
/*!
 * \}
 */	// end of group IuEyePhotometry

// ============================================================================
/*! \defgroup IuEyeAutoFramerate uEye Auto Framerate Interface
*  Proprietary interface for uEye auto framerate feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the sensor's or driver's
*  auto framerate feature related parameters that are not accessible via direct show functions.
*  \note you can use either sensor's or driver's auto framerate functionality, not both.
* \{
*/
// ============================================================================

// {92931A38-35C1-4923-97CC-0BCEE403EAFA}
DEFINE_GUID(IID_IuEyeAutoFramerate, 
            0x92931a38, 0x35c1, 0x4923, 0x97, 0xcc, 0xb, 0xce, 0xe4, 0x3, 0xea, 0xfa);

interface IuEyeAutoFramerate : public IUnknown
{
    /*!
     * \brief Query for support of sensor's auto framerate feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoFramerateSensor_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of sensor's auto framerate feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoFramerateSensor_IsEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of sensor's auto framerate feature.
     * \param bEnable   new 'enabled' status.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoFramerateSensor_Enable)(bool bEnable) = 0;
    /*!
     * \brief Query for support of driver's auto framerate feature.
     * \param pbSupported   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoFramerateDriver_IsSupported)(bool* pbSupported) = 0;

    /*!
     * \brief Query the current 'enabled' status of driver's auto framerate feature.
     * \param pbEnabled output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoFramerateDriver_IsEnabled)(bool* pbEnabled) = 0;

    /*!
     * \brief Set the 'enabled' status of driver's auto framerate feature.
     * \param bEnable   new 'enabled' status.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoFramerateDriver_Enable)(bool bEnable) = 0;
    /*!
     * \brief Get the actual framerate of the camera.
     * \param dblFramerate   current framerate
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(AutoFramerate_GetFramerate)(double* dblFramerate) = 0;
};
/*!
 * \}
 */	// end of group IuEyeAutoFramerate

// ============================================================================
/*! \defgroup IuEyeFlash uEye Flash Interface
*  Proprietary interface for uEye flash feature exposed by the capture filter. 
*  Allows a DirectShow based program to control and query the flash feature related
*  parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {051D5440-05F8-498f-BDAD-19E3ABB48ED9}
DEFINE_GUID(IID_IuEyeFlash, 0x51d5440, 0x5f8, 0x498f, 0xbd, 0xad, 0x19, 0xe3, 0xab, 0xb4, 0x8e, 0xd9);


interface IuEyeFlash : public IUnknown
{
    /*!
     * \brief Set the current flash strobe mode.
     * \param lMode     flash strobe mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_SetStrobeMode)(long lMode) = 0;

    /*!
     * \brief Query the current flash strobe mode.
     * \param plMode    output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_GetStrobeMode)(long* plMode) = 0;

    /*!
     * \brief Set flash duration
	* \param	pulDuration	flash duration to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_GetDuration)(unsigned long* pulDuration) = 0;

    /*!
     * \brief Query the range of flash duration
     * \param pulMin    minimum value
     * \param pulMax    maximum value
     * \param pulInc    step width
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_GetDurationRange)(unsigned long* pulMin, unsigned long* pulMax, unsigned long* pulInc ) = 0;

    /*!
     * \brief Set flash delay.
     * \param ulDelay   flash delay to be set.
	* \param	ulDuration	flash duration to be set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_SetDelayDuration)(unsigned long ulDelay, unsigned long ulDuration) = 0;

    /*!
     * \brief Query current flash delay.
     * \param pulDelay   output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_GetDelay)(unsigned long* pulDelay) = 0;

    /*!
     * \brief Query the range of flash delay
     * \param pulMin    minimum value
     * \param pulMax    maximum value
     * \param pulInc    step width
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_GetDelayRange)(unsigned long* pulMin, unsigned long* pulMax, unsigned long* pulInc ) = 0;

    /*!
     * \brief Query global exposure window to simulate global shutter
     * \param pulDelay      delay used for simulating global shutter
     * \param pulDuration   duration used for simulating global shutter
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_GetGlobalExposureWindow)(unsigned long* pulDelay, unsigned long* pulDuration ) = 0;

    /*!
     * \brief Query supported gpio ports available for flash output
     * \param pulPorts  bitmask containing supported gpio ports
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_GetSupportedGPIOPorts)(unsigned long* pulPorts ) = 0;

    /*!
     * \brief enable flash output on spcified gpio port
     * \param ulPort    port to be used for flash purposes
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Flash_EnableGPIOPort)(unsigned long ulPort ) = 0;
};

/*!
 * \}
 */	// end of group IuEyeFlash

// ============================================================================
/*! \defgroup IuEyeResample uEye Subsampling and Binning Interface
*  Proprietary interface for uEye subsampling and binning features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the subsampling and
*  binning feature related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {7C0098F5-20BE-47f7-83FF-E7CC12246547}
DEFINE_GUID(IID_IuEyeResample, 0x7c0098f5, 0x20be, 0x47f7, 0x83, 0xff, 0xe7, 0xcc, 0x12, 0x24, 0x65, 0x47);
        
interface IuEyeResample : public IUnknown
{
    /*!
     * \brief Set the current subsampling mode.
     * \param lMode     flash strobe mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_SetMode)(long lMode) = 0;

    /*!
     * \brief Query the current subsampling mode.
     * \param plMode    output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_GetMode)(long* plMode) = 0;

    /*!
     * \brief Query current vertical resolution.
     * \param pulResolution   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_GetVerticalResolution)(unsigned long* pulResolution) = 0;

    /*!
     * \brief Query current horizontal resolution.
     * \param pulResolution   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_GetHorizontalResolution)(unsigned long* pulResolution) = 0;

    /*!
     * \brief Query if 2x vertical subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is2xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 2x horizontal subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is2xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 3x vertical subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is3xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 3x horizontal subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is3xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 4x vertical subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is4xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 4x horizontal subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is4xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 5x vertical subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is5xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 5x horizontal subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is5xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 6x vertical subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is6xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 6x horizontal subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is6xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 8x vertical subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is8xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 8x horizontal subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is8xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 16x vertical subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is16xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 16x horizontal subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_Is16xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if colorful subsampling is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Subsampling_IsColorSubsamplingSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Set the current binning mode.
     * \param lMode     binning mode to set.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_SetMode)(long lMode) = 0;

    /*!
     * \brief Query the current binning mode.
     * \param plMode    output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_GetMode)(long* plMode) = 0;

    /*!
     * \brief Query current vertical resolution.
     * \param pulResolution   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_GetVerticalResolution)(unsigned long* pulResolution) = 0;

    /*!
     * \brief Query current horizontal resolution.
     * \param pulResolution   output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_GetHorizontalResolution)(unsigned long* pulResolution) = 0;

    /*!
     * \brief Query current image width.
     * \param pnWidth   output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_GetImageWidth)(int* pnWidth) = 0;

    /*!
     * \brief Query current image height.
     * \param pnHeight   output location for result.
	* \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_GetImageHeight)(int* pnHeight) = 0;

    /*!
     * \brief Query if 2x vertical binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is2xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 2x horizontal binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is2xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 3x vertical binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is3xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 3x horizontal binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is3xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 4x vertical binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is4xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 4x horizontal binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is4xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 6x vertical binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is6xVertSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if 6x horizontal binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_Is6xHorSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if colorful binning is supported
     * \param pbSupported  output location for result.
	* \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Binning_IsColorBinningSupported)(bool* pbSupported ) = 0;
};
/*!
 * \}
 */	// end of group IuEyeResample

// ============================================================================
/*! \defgroup IuEyeAOI uEye Area of Interest Interface
*  Proprietary interface for controlling uEye AOI features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the AOI feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================
// {8201BA4C-6E10-4258-9E4E-F8A70DFF4FAF}
DEFINE_GUID(IID_IuEyeAOI, 0x8201ba4c, 0x6e10, 0x4258, 0x9e, 0x4e, 0xf8, 0xa7, 0xd, 0xff, 0x4f, 0xaf);

interface IuEyeAOI : public IUnknown
{
    /*!
     * \brief   Query if setting an image aoi is supported
     * \param   pbSupported  output location for result.
	 * \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_IsImageAOISupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Get the current image area of interest in absolute uEye coordinates.
     * \param   prcAOI   output location containing aoi information (left, top, right, bottom).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetImageAOI)(RECT *prcAOI) = 0;

    /*!
     * \brief Set the current image area of interest in absolute uEye coordinates.
     * \param   rcAOI   variable of type RECT containing the new AOI coordinates.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_SetImageAOI)(RECT rcAOI) = 0;

    /*!
     * \brief Get the current area of interest used by auto exposure feature (if not set the actual image AOI is used).
     * \param   prcAOI   output location containing aoi information (left, top, right, bottom).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetAutoBrightnessAOI)(RECT *prcAOI) = 0;

    /*!
     * \brief Set the current area of interest used by auto exposure feature.
     * \param   rcAOI   variable of type RECT containing the new AOI coordinates.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_SetAutoBrightnessAOI)(RECT rcAOI) = 0;

    /*!
     * \brief Get the current area of interest used by auto white balance feature (if not set the actual image AOI is used).
     * \param   prcAOI   output location containing aoi information (left, top, right, bottom).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetAutoWBAOI)(RECT *prcAOI) = 0;

    /*!
     * \brief Set the current area of interest used by auto white balance feature.
     * \param   rcAOI   variable of type RECT containing the new AOI coordinates.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_SetAutoWBAOI)(RECT rcAOI) = 0;

    /*!
     * \brief Get the increment to change horizontal position of the AOI.
     * \param   pnInc   variable containing the increment value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetIncPosX)(INT* pnInc) = 0;

    /*!
     * \brief Get the increment to change vertical position of the AOI.
     * \param   pnInc   variable containing the increment value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetIncPosY)(INT* pnInc) = 0;

    /*!
     * \brief Get the increment to change the width of the AOI.
     * \param   pnInc   variable containing the increment value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetIncSizeX)(INT* pnInc) = 0;

    /*!
     * \brief Get the increment to change the height of the AOI.
     * \param   pnInc   variable containing the increment value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetIncSizeY)(INT* pnInc) = 0;

    /*!
     * \brief Get the minimum and maximum value of the horizontal position of the AOI.
     * \param   pnMin   variable containing the smallest possible horizontal position.
     * \param   pnMax   variable containing the largest possible horizontal position.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetMinMaxPosX)(INT* pnMin, INT* pnMax) = 0;

    /*!
     * \brief Get the minimum and maximum value of the vertical position of the AOI.
     * \param   pnMin   variable containing the smallest possible horizontal position.
     * \param   pnMax   variable containing the largest possible horizontal position.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetMinMaxPosY)(INT* pnMin, INT* pnMax) = 0;

    /*!
     * \brief Get the minimum and maximum value of the width of the AOI.
     * \param   pnMin   variable containing the smallest possible width.
     * \param   pnMax   variable containing the largest possible width.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetMinMaxSizeX)(INT* pnMin, INT* pnMax) = 0;


    /*!
     * \brief Get the minimum and maximum value of the height of the AOI.
     * \param   pnMin   variable containing the smallest possible height.
     * \param   pnMax   variable containing the largest possible height.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetMinMaxSizeY)(INT* pnMin, INT* pnMax) = 0;

    /*!
     * \brief Generic access to the AOI interface. Use this command to access to more AOI functionality. (see uEye SDK documentation)
     * \param   ulCommand   specifies which aoi command has to be used.
     * \param   pParam      void pointer containing function parameters.
     * \param   ulSize      size of pParam.
     * \return  HRESULT	S_OK on success, error code otherwise.
     * \note the generic access function is provided for future use.
     */
    STDMETHOD(AOI_Generic)(unsigned long ulCommand, void* pParam, unsigned long ulSize) = 0;

    /*!
     * \brief Set the current image area of interest in absolute uEye coordinates.
     * \param   rectAOI   variable of type IS_RECT containing the new AOI coordinates.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_SetImageAOI)(IS_RECT rectAOI) = 0;

    /*!
     * \brief Get the current image area of interest in absolute uEye coordinates.
     * \param   pRectAOI   output location containing aoi information (x, y, width, height).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetImageAOI)(IS_RECT *pRectAOI) = 0;

    /*!
     * \brief Get the current area of interest used by auto exposure feature (if not set the actual image AOI is used).
     * \param   pRectAOI   output location containing aoi information (x, y, width, height).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD (AOI_GetAutoBrightnessAOI)(IS_RECT *pRectAOI) = 0;

    /*!
     * \brief Set the current area of interest used by auto exposure feature.
     * \param   rectAOI   variable of type IS_RECT containing the new AOI coordinates.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_SetAutoBrightnessAOI)(IS_RECT rectAOI) = 0;

    /*!
     * \brief Get the current area of interest used by auto white balance feature (if not set the actual image AOI is used).
     * \param   pRectAOI   output location containing aoi information (x, y, width, height).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_GetAutoWBAOI)(IS_RECT *pRectAOI) = 0;

    /*!
     * \brief Set the current area of interest used by auto white balance feature.
     * \param   rectAOI   variable of type IS_RECT containing the new AOI coordinates.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(AOI_SetAutoWBAOI)(IS_RECT rectAOI) = 0;

};
/*!
 * \}
 */	// end of group IuEyeAOI

// ============================================================================
/*! \defgroup IuEyeGain uEye Hardware Gain Interface
*  Proprietary interface for controlling uEye hardware gain features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the hardware gain feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================
// {D750DAE4-9C88-48cd-B966-79ACE3E5147C}
DEFINE_GUID(IID_IuEyeGain, 0xd750dae4, 0x9c88, 0x48cd, 0xb9, 0x66, 0x79, 0xac, 0xe3, 0xe5, 0x14, 0x7c);

interface IuEyeGain : public IUnknown
{
    /*!
     * \brief Query if master gain is supported
     * \param pbSupported  output location for result.
	 * \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_IsMasterSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief Query if rgb gains are supported
     * \param pbSupported  output location for result.
	 * \return	HRESULT		S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_IsRGBSupported)(bool* pbSupported ) = 0;

    /*!
     * \brief   Get the current hardware gain value.
     * \param   nWhich  specifies which gain to query (0 = master gain, 1 = red gain, 
     *                  2 = green gain, 3 = blue gain).
     * \param   pnValue output location containing gain value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_GetHwGain)(INT nWhich, INT *pnValue) = 0;

    /*!
     * \brief   Set the specified hardware gain value.
     * \param   nWhich  specifies which gain to set (0 = master gain, 1 = red gain, 
     *                  2 = green gain, 3 = blue gain).
     * \param   nValue  gain value to set.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_SetHwGain)(INT nWhich, INT nValue) = 0;

    /*!
     * \brief   Get the hardware gain default values.
     * \param   pnMaster    output location containing the master gain default value.
     * \param   pnRed       output location containing the red gain default value (-1 if not available).
     * \param   pnGreen     output location containing the green gain default value (-1 if not available).
     * \param   pnBlue      output location containing the blue gain default value (-1 if not available).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_GetHwGainDefaults)(INT *pnMaster, INT *pnRed, INT *pnGreen, INT *pnBlue) = 0;

    /*!
     * \brief   Get the hardware gain value range.
     * \param   nWhich  specifies which gain to query (0 = master gain, 1 = red gain, 
     *                  2 = green gain, 3 = blue gain).
     * \param   pnMin output location containing minimum gain value.
     * \param   pnMin output location containing maximum gain value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_GetHwGainRange)(INT nWhich, INT *pnMin, INT *pnMax) = 0;

    /*!
     * \brief   Get the current gain factor value.
     * \param   nWhich      specifies which gain factor to query (0 = master gain, 1 = red gain, 
     *                      2 = green gain, 3 = blue gain).
     * \param   pnFactor    output location containg the queried gain factor value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_GetHwGainFactor)(INT nWhich, INT* pnFactor) = 0;

    /*!
     * \brief   Set the specified hardware gain factor value.
     * \param   nWhich  specifies which gain to set (0 = master gain, 1 = red gain, 
     *                  2 = green gain, 3 = blue gain).
     * \param   nValue  gain factor value to set.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_SetHwGainFactor)(INT nWhich, INT nFactor) = 0;

    /*!
     * \brief   Get the hardware gain factor default values.
     * \param   pnMasterFactor    output location containing the master gain factor default value.
     * \param   pnRedFactor       output location containing the red gain factor default value (-1 if not available).
     * \param   pnGreenFactor     output location containing the green gain factor default value (-1 if not available).
     * \param   pnBlueFactor      output location containing the blue gain factor default value (-1 if not available).
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_GetHwGainFactorDefaults)(INT *pnMasterFactor, INT *pnRedFactor, INT *pnGreenFactor, INT *pnBlueFactor) = 0;

    /*!
     * \brief   Get the current gain factor value for the given gain index.
     * \param   nWhich      specifies which gain factor to query (0 = master gain, 1 = red gain, 
     *                      2 = green gain, 3 = blue gain).
     * \param   nGain       gain value to query factor for.
     * \param   pnFactor    output location containing the queried gain factor value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_InquireHwGainFactor)(INT nWhich, INT nGain, INT* pnFactor) = 0;

    /*!
     * \brief   Get the gain factor value range for the given gain.
     * \param   nWhich      specifies which gain factor to query (0 = master gain, 1 = red gain, 
     *                      2 = green gain, 3 = blue gain).
     * \param   pnMin       output location containing the minimum gain factor value.
     * \param   pnMax       output location containing the maximum gain factor value.
	 * \return	HRESULT	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_GetHwGainFactorRange)(INT nWhich, INT* pnMin, INT* pnMax) = 0;

    /*!
     * \brief   Query for support of sensor's gain boost feature.
     * \param   pbSupported     output location for result.
	 * \return	HRESULT 		S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_IsGainBoostSupported)(bool* pbSupported) = 0;

    /*!
     * \brief   Query the current value of sensor's gain boost feature.
     * \param   plValue     current value.
	* \return	HRESULT     S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_GetGainBoostValue)(long* plValue) = 0;

    /*!
     * \brief   Set the saturation value of sensor's saturation feature.
     * \param   lValue      mode to set.
	* \return	HRESULT 	S_OK on success, error code otherwise.
     */
    STDMETHOD(Gain_SetGainBoostValue)(long lValue) = 0;

};
/*!
 * \}
 */	// end of group IuEyeGain


// ============================================================================
/*! \defgroup IuEyeScaler uEye Scaler Interface
*
*
*
* \{
*/
// ============================================================================

// {720C5C49-5282-4b6e-9FED-98FE8A8A6063}
DEFINE_GUID(IID_IuEyeScaler, 0x720c5c49, 0x5282, 0x4b6e, 0x9f, 0xed, 0x98, 0xfe, 0x8a, 0x8a, 0x60, 0x63);

interface IuEyeScaler : public IUnknown
{
	/*!	 
	* \brief	Activated in some sensors the internal image scaling
     *	\param 	nMode		   Function mode
	 *          dblFactor	   scaling factor
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(SetSensorScaler)(UINT nMode, double dblFactor) = 0;

	/*!	 
	* \brief	Obtained from some sensors information about the internal image scaling
     *	\param 	pSensorScalerInfo		Pointer to a structure of type SENSORSCALERINFO, 
	 *									where should be written in the information
	 *          nSensorScalerInfoSize   Size of the structure of type SENSORSCALERINFO
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(GetSensorScalerInfo)(SENSORSCALERINFO *pSensorScalerInfo, INT nSensorScalerInfoSize) = 0;

	STDMETHOD(GetScalerImageWidth)		(int *pnWidth) = 0;
	STDMETHOD(GetScalerImageHeight)		(int *pnHeight) = 0;
	STDMETHOD(SetImageSize)				(int nWidth, int nHeight) = 0;
};
/*!
 * \}
 */	// end of group IuEyeScaler

// ============================================================================
/*! \defgroup IuEyeEvent uEye Event Interface
*
*
*
* \{
*/
// ============================================================================

// {EB1EF72D-9A55-4830-94D0-AAC21B2CE7B9}
DEFINE_GUID(IID_IuEyeEvent, 0xeb1ef72d, 0x9a55, 0x4830, 0x94, 0xd0, 0xaa, 0xc2, 0x1b, 0x2c, 0xe7, 0xb9);

interface IuEyeEvent : public IUnknown
{

	/*!	 
	 *  \brief	Initialising Event
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(InitEvent)(HANDLE hEv, INT nWhich) = 0;

	/*!	 
	 *  \brief	Enable Event
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(EnableEvent)(INT nWhich) = 0;

	/*!	 
	 *  \brief	Disable Event
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(DisableEvent)(INT nWhich) = 0;

    /*!	 
	 *  \brief	Exit Event
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(ExitEvent)(INT nWhich) = 0;

    /*!	 
	 *  \brief	EnableMessage
	 *
     *	\return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD(EnableMessage)(INT which, HWND hWnd) = 0;

};
/*!
 * \}
 */	// end of group IuEyeEvent

// ============================================================================
/*! \defgroup IuEyeDeviceFeature uEye DeviceFeature Interface
* 
*  Proprietary interface for controlling uEye device feature features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the device feature feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================
// {0F0BC8F8-D210-45a7-AA18-D1377A56E158}
DEFINE_GUID(IID_IuEyeDeviceFeature, 0xf0bc8f8, 0xd210, 0x45a7, 0xaa, 0x18, 0xd1, 0x37, 0x7a, 0x56, 0xe1, 0x58);

interface IuEyeDeviceFeature : public IUnknown
{
	/*!	 
	 * \brief	get the supported device features
     * \param 	pnCap		   supported device features
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetSupportedFeatures)(INT* pnCap) = 0;

	/*!	 
	 * \brief	set the linescan mode 
     * \param 	nMode		linescane mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetLineScanMode)(INT nMode) = 0;

	/*!	 
	 * \brief	get the linescan mode 
     * \param 	pnMode		linescane mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLineScanMode)(INT* pnMode) = 0;

	/*!	 
	 * \brief	set the linescan number 
     * \param 	nNumber		linescane number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetLineScanNumber)(INT nNumber) = 0;

	/*!	 
	 * \brief	get the linescan number 
     * \param 	pnNumber	linescane number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLineScanNumber)(INT* pnNumber) = 0;

	/*!	 
	 * \brief	set the shutter mode 
     * \param 	nMode		shutter mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetShutterMode)(INT nMode) = 0;

	/*!	 
	 * \brief	get the shutter mode 
     * \param 	pnMode		shutter mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetShutterMode)(INT* pnMode) = 0;

	/*!	 
	 * \brief	set the prefer XS Hs mode 
     * \param 	nMode		prefer XS HS mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetPreferXSHSMode)(INT nMode) = 0;

	/*!	 
	 * \brief	get the prefer XS Hs mode 
     * \param 	pnMode		prefer XS HS mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetPreferXSHSMode)(INT* pnMode) = 0;

	/*!	 
	 * \brief	get the prefer XS Hs default mode 
     * \param 	pnDefault	prefer XS HS default mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetDefaultPreferXSHSMode)(INT* pnDefault) = 0;

	/*!	 
	 * \brief	get the default log mode 
     * \param 	pnDefault	default log mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetDefaultLogMode)(UINT* pnDefault) = 0;

	/*!	 
	 * \brief	set the log mode 
     * \param 	nMode		log mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetLogMode)(UINT nMode) = 0;

	/*!	 
	 * \brief	get the log mode 
     * \param 	pnMode		log mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLogMode)(UINT* pnMode) = 0;

	/*!	 
	 * \brief	get the log mode manual default value
     * \param 	pnDefault	log mode manual default value
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLogModeManualValueDefault)(UINT* pnDefault) = 0;

	/*!	 
	 * \brief	get the log mode manual value range
     * \param 	pnMin		minimum log mode manual value
	 * \		pnMax		maximum log mode manual value
	 * \		pnInc		increment log mode manual value
	 * 
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLogModeManualValueRange)(INT* pnMin, INT* pnMax, INT* pnInc) = 0;

	/*!	 
	 * \brief	set the log mode manual value
     * \param 	nValue		log mode manual value
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetLogModeManualValue)(UINT nValue) = 0;

	/*!	 
	 * \brief	get the log mode manual value
     * \param 	pnValue		log mode manual value
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLogModeManualValue)(UINT* pnValue) = 0;

	/*!	 
	 * \brief	get the log mode manual default gain
     * \param 	pnDefault	log mode manual default gain
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLogModeManualGainDefault)(UINT* pnDefault) = 0;

	/*!	 
	 * \brief	get the log mode manual gain range
     * \param 	pnMin		minimum log mode manual gain
	 * \		pnMax		maximum log mode manual gain
	 * \		pnInc		increment log mode manual gain
	 * 
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLogModeManualGainRange)(INT* pnMin, INT* pnMax, INT* pnInc) = 0;

	/*!	 
	 * \brief	set the log mode manual gain
     * \param 	nGain		log mode manual gain
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetLogModeManualGain)(UINT nGain) = 0;

	/*!	 
	 * \brief	get the log mode manual gain
     * \param 	pnGain		log mode manual gain
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetLogModeManualGain)(UINT* pnGain) = 0;

	/*!
	 * \brief	get the vertical aoi merge mode
	 * \param	pnMode 		vertical AOI merge mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
     */
	STDMETHOD (DeviceFeature_GetVerticalAOIMergeMode)(INT* pnMode) = 0;		

	/*!
	 * \brief	set the vertical aoi merge mode
	 * \param	nMode 		vertical AOI merge mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
     */
	STDMETHOD (DeviceFeature_SetVerticalAOIMergeMode)(INT nMode) = 0;	

	/*!
	 * \brief	get the vertical aoi merge position
	 * \param	pnPosition  vertical AOI merge position
	 *
     * \return	HRESULT     0 on success, error code otherwise.
     */
	STDMETHOD (DeviceFeature_GetVerticalAOIMergePosition)(INT* pnPosition) = 0;		

	/*!
	 * \brief	set the vertical aoi merge position
	 * \param	nPosition   vertical AOI merge position
	 *
     * \return	HRESULT     0 on success, error code otherwise.
     */
	STDMETHOD (DeviceFeature_SetVerticalAOIMergePosition)(INT nPosition) = 0;	

	/*!
	 * \brief	get default FPN correction mode
	 * \param	pnMode		get default FPN correction mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetDefaultFPNCorrectionMode)(UINT* pnMode) = 0;

	/*!
	 * \brief	get FPN correction mode
	 * \param	pnMode		get FPN correction mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetFPNCorrectionMode)(UINT* pnMode) = 0;

	/*!
	 * \brief	set FPN correction mode
	 * \param	nMode		set FPN correction mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetFPNCorrectionMode)(UINT nMode) = 0;

	/*!
	 * \brief	sensor source gain range
	 * \param	pnMin		minimum sensor source gain
	 * \		pnMax		maximum sensor source gain
	 * \		pnInc		increment sensor source gain
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetSensorSourceGainRange)(INT* pnMin, INT* pnMax, INT* pnInc) = 0;

	/*!
	 * \brief	default sensor source gain
	 * \param	pnGain		default sensor source gain
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetSensorSourceGainDefault)(INT* pnGain) = 0;

	/*!
	 * \brief	get sensor source gain
	 * \param	pnGain		get sensor source gain
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetSensorSourceGain)(INT* pnGain) = 0;

	/*!
	 * \brief	set sensor source gain
	 * \param	pnGain		set sensor source gain
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetSensorSourceGain)(INT nGain) = 0;

	/*!
	 * \brief	get black reference mode
	 * \param	pnMode		get black reference mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetBlackReferenceMode)(UINT* pnMode) = 0;

	/*!
	 * \brief	set black reference mode
	 * \param	nMode		set black reference mode
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetBlackReferenceMode)(UINT nMode) = 0;

	/*!
	 * \brief	get allow raw with LUT
	 * \param	pnAllowRawWithLut	get allow raw with LUT
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetAllowRawWithLUT)(UINT* pnAllowRawWithLut) = 0;

	/*!
	 * \brief	set allow raw with LUT
	 * \param	nAllowRawWithLut	set allow raw with LUT
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetAllowRawWithLUT)(UINT nAllowRawWithLut) = 0;

	/*!
	 * \brief	get supported sensor bit depth
	 * \param	pnSupported		get supported sensor bit depth
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetSensorBitDepthSupported)(UINT* pnSupported) = 0;

	/*!
	 * \brief	get default sensor bit depth
	 * \param	pnDefault		get default sensor bit depth
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetSensorBitDepthDefault)(UINT* pnDefault) = 0;

	/*!
	 * \brief	get sensor bit depth
	 * \param	pnBitDepth		get sensor bit depth
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetSensorBitDepth)(UINT* pnBitDepth) = 0;

	/*!
	 * \brief	set sensor bit depth
	 * \param	nBitDepth		set sensor bit depth
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetSensorBitDepth)(UINT nBitDepth) = 0;

	/*!
	 * \brief	get default image effect mode
	 * \param	pnImageEffect	get default image effect mode
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetImageEffectModeDefault)(INT* pnImageEffect) = 0;

	/*!
	 * \brief	get image effect mode
	 * \param	pnImageEffect	get image effect mode
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetImageEffectMode)(INT* pnImageEffect) = 0;

	/*!
	 * \brief	set image effect mode
	 * \param	nImageEffect	set image effect mode
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetImageEffectMode)(INT nImageEffect) = 0;

	/*!
	 * \brief	get JPEG compression range
	 * \param	pnMin		minimum JPEG compression
	 * \		pnMax		maximum JPEG compression
	 * \		pnInc		increment JPEG compression
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetJPEGCompressionRange)(INT* pnMin, INT* pnMax, INT* pnInc) = 0;

	/*!
	 * \brief	get default JPEG compression 
	 * \param	pnDefault	default JPEG compression value
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetJPEGCompressionDefault)(INT* pnDefault) = 0;

	/*!
	 * \brief	get JPEG compression 
	 * \param	pnValue		get JPEG compression value
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetJPEGCompression)(INT* pnValue) = 0;

	/*!
	 * \brief	set JPEG compression 
	 * \param	nValue		set JPEG compression value
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetJPEGCompression)(INT nValue) = 0;

	/*!
	 * \brief	get noise reduction mode default 
	 * \param	piNoiseReductionDefault		get default noise reduction mode
	 *
     * \return	HRESULT						0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetNoiseReductionModeDefault)(INT* piNoiseReductionDefault) = 0;

	/*!
	 * \brief	get noise reduction mode
	 * \param	piNoiseReduction		get noise reduction 
	 *
     * \return	HRESULT					0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_GetNoiseReductionMode)(INT* piNoiseReduction) = 0;

	/*!
	 * \brief	set noise reduction mode
	 * \param	iNoiseReduction		set noise reduction 
	 *
     * \return	HRESULT				0 on success, error code otherwise.
	 */
	STDMETHOD (DeviceFeature_SetNoiseReductionMode)(INT iNoiseReduction) = 0;
};

/* 27.11.2012 */
// ============================================================================
/*! \defgroup IuEyeHotpixel uEye Hotpixel Interface
* 
*  Proprietary interface for controlling uEye hotpixel features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the hotpixel feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {8916BFCA-CB66-455f-8AC3-752EBC1D76D5}
DEFINE_GUID(IID_IuEyeHotPixel, 0x8916bfca, 0xcb66, 0x455f, 0x8a, 0xc3, 0x75, 0x2e, 0xbc, 0x1d, 0x76, 0xd5);

interface IuEyeHotPixel : public IUnknown
{
	/*!	 
	 * \brief	disable hotpixel correction
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_DisableCorrection)(void) = 0;

	/*!	 
	 * \brief	enable camera hotpixel correction
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_EnableCameraCorrection)(void) = 0;

	/*!	 
	 * \brief	enable software user hotpixel correction
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_EnableSoftwareUserCorrection)(void) = 0;

	/*!	 
	 * \brief	enable or disable sensor hotpixel correction
     * \param 	bEnable		enable / disable
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_SensorCorrection)(bool bEnable) = 0;

	/*!	 
	 * \brief	get hotpixel correction modes
     * \param 	nMode		correction modes
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCorrectionMode)(INT* pnMode) = 0;

	/*!	 
	 * \brief	get supported hotpixel correction modes
     * \param 	nMode		correction modes
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetSupportedCorrectionModes)(INT* pnMode) = 0;

	/*!	 
	 * \brief	get software user list exist
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetSoftwareUserListExist)(void) = 0;

	/*!	 
	 * \brief	get software user list number
     * \param 	pnNumber	user list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetSoftwareUserListNumber)(INT* pnNumber) = 0;

	/*!	 
	 * \brief	get software user list 
     * \param 	pList		user list 
	 *			nNumber		user list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetSoftwareUserList)(WORD *pList, INT nNumber) = 0;

	/*!	 
	 * \brief	set software user list 
     * \param 	pList		user list 
	 *			nNumber		user list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_SetSoftwareUserList)(WORD *pList, INT nNumber) = 0;

	/*!	 
	 * \brief	save software user list to file
     * \param 	pFile		file
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_SaveUserList)(char* pFile) = 0;

	/*!	 
	 * \brief	load software user list from file
     * \param 	pFile		file
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_LoadUserList)(char* pFile) = 0;

	/*!	 
	 * \brief	save software user list to file
     * \param 	pFile		file
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_SaveUserListUnicode)(wchar_t* pFile) = 0;

	/*!	 
	 * \brief	load software user list from file
     * \param 	pFile		file
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_LoadUserListUnicode)(wchar_t* pFile) = 0;

	/*!	 
	 * \brief	get camera factory list exist
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCameraFactoryListExist)(void) = 0;

	/*!	 
	 * \brief	get camera factory list number
     * \param 	pnNumber	factory list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCameraFactoryListNumber)(INT* pnNumber) = 0;

	/*!	 
	 * \brief	get camera factory list 
     * \param 	pList		factory list 
	 *			nNumber		factory list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCameraFactoryList)(WORD *pList, INT nNumber) = 0; 

	/*!	 
	 * \brief	get camera user list exist
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCameraUserListExist)(void) = 0;

	/*!	 
	 * \brief	get camera user list number
     * \param 	pnNumber	user list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCameraUserListNumber)(INT* pnNumber) = 0;

	/*!	 
	 * \brief	get camera user list 
     * \param 	pList		user list 
	 *			nNumber		user list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCameraUserList)(WORD *pList, INT nNumber) = 0;

	/*!	 
	 * \brief	set camera user list 
     * \param 	pList		user list 
	 *			nNumber		user list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_SetCameraUserList)(WORD *pList, INT nNumber) = 0;

	/*!	 
	 * \brief	delete camera user list 
     * \param 	void		
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_DeleteCameraUserList)(void) = 0;

	/*!	 
	 * \brief	get camera user list  max number
     * \param 	pnNumber	user list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetCameraUserListMaxNumber)(INT* pnNumber) = 0;

	/*!	 
	 * \brief	get merged camera list number
     * \param 	pnNumber	merged list number
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetMergedCameraListNumber)(INT* pnNumber) = 0;

	/*!	 
	 * \brief	get merged camera list
     * \param 	pList		merged list 
	 *			nNumber		merged list number
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (HotPixel_GetMergedCameraList)(WORD *pList, INT nNumber)= 0;
};


/* 16.01.2013 CameraLUT */
// ============================================================================
/*! \defgroup IuEyeCameraLUT uEye CameraLUT Interface
* 
*  Proprietary interface for controlling uEye camera LUT features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the camera LUT feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {15E696DE-5D63-4ea8-AA66-9C2528544DB1}
DEFINE_GUID(IID_IuEyeCameraLUT, 0x15e696de, 0x5d63, 0x4ea8, 0xaa, 0x66, 0x9c, 0x25, 0x28, 0x54, 0x4d, 0xb1);

interface IuEyeCameraLUT : public IUnknown
{
	/*!	 
	 * \brief	set camera LUT
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (SetCameraLUT)(UINT nMode, UINT nNumberOfEntries, double* pRed_Grey, double* pGreen, double* pBlue) = 0;

	/*!	 
	 * \brief	get camera LUT
     * \param 	void
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (GetCameraLUT)(UINT nMode, UINT nNumberOfEntries, double* pRed_Grey, double* pGreen, double* pBlue) = 0;
};

// ============================================================================
/*! \defgroup IuEyeDeviceFeature uEye EdgeEnhancement Interface
* 
*  Proprietary interface for controlling uEye EdgeEnhancement features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the device feature feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================
// {BBF3B063-81BE-4ed2-ABCA-3552B462EEBA}
DEFINE_GUID(IID_IuEyeEdgeEnhancement, 0xbbf3b063, 0x81be, 0x4ed2, 0xab, 0xca, 0x35, 0x52, 0xb4, 0x62, 0xee, 0xba);

interface IuEyeEdgeEnhancement : public IUnknown
{
	/*!	 
	 * \brief	get edge enhancement range
     * \param 	pnMin		minimum edge enhancement
	 * \param	pnMax		maximum edge enhancement
	 * \param	pnInc		increment edge enhancement
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (GetEdgeEnhancementRange)(UINT* pnMin, UINT* pnMax, UINT* pnInc) = 0;

	/*!	 
	 * \brief	get default edge enhancement
     * \param 	pnDefault	default edge enhancement
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (GetEdgeEnhancementDefault)(UINT* pnDefault) = 0;

	/*!	 
	 * \brief	get edge enhancement
     * \param 	pnEdgeEnhancement	get edge enhancement
	 *
     * \return	HRESULT				0 on success, error code otherwise.
	 */
	STDMETHOD (GetEdgeEnhancement)(UINT* pnEdgeEnhancement) = 0;

	/*!	 
	 * \brief	set edge enhancement
     * \param 	nEdgeEnhancement	set edge enhancement
	 *
     * \return	HRESULT				0 on success, error code otherwise.
	 */
	STDMETHOD (SetEdgeEnhancement)(UINT nEdgeEnhancement) = 0;
};


// ============================================================================
/*! \defgroup IuEyeAutoParameter uEye auto parameter Interface
* 
*  Proprietary interface for controlling uEye auto parameter features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the device feature feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================
// {0917C6E0-D724-4f09-98A9-57F0A5412D65}
DEFINE_GUID(IID_IuEyeAutoParameter, 0x917c6e0, 0xd724, 0x4f09, 0x98, 0xa9, 0x57, 0xf0, 0xa5, 0x41, 0x2d, 0x65);

interface IuEyeAutoParameter : public IUnknown
{
	/*!	 
	 * \brief	get supported AWB types
     * \param 	pnTypes		get supported AWB types
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_GetSupportedAWBTypes)(UINT* pnTypes) = 0;

	/*!	 
	 * \brief	get AWB type
     * \param 	pnType		get AWB type
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_GetAWBType)(UINT* pnType) = 0;

	/*!	 
	 * \brief	set AWB type
     * \param 	nType		set AWB type
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_SetAWBType)(UINT nType) = 0;

	/*!	 
	 * \brief	get enable AWB
     * \param 	pnEnable	get enable AWB
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_GetEnableAWB)(UINT* pnEnable) = 0;

	/*!	 
	 * \brief	set enable AWB
     * \param 	nEnable		set enable AWB
	 *
     * \return	HRESULT     0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_SetEnableAWB)(UINT nEnable) = 0;

	/*!	 
	 * \brief	get supported RGB color model AWB
     * \param 	pnSupported		get supported RGB color model AWB
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_GetSupportedRGBColorModelAWB)(UINT* pnSupported) = 0;

	/*!	 
	 * \brief	get RGB color model AWB
     * \param 	pnColorModel	get RGB color model AWB
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_GetRGBColorModelAWB)(UINT* pnColorModel) = 0;

	/*!	 
	 * \brief	set RGB color model AWB
     * \param 	nColorModel		set RGB color model AWB
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (AutoParameter_SetRGBColorModelAWB)(UINT nColorModel) = 0;

};


// ============================================================================
/*! \defgroup IuEyeImageFormat uEye image format Interface
* 
*  Proprietary interface for controlling uEye image format features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the device feature feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {368F0072-66D3-406f-A715-D8128EAD9B7D}
DEFINE_GUID(IID_IuEyeImageFormat, 
0x368f0072, 0x66d3, 0x406f, 0xa7, 0x15, 0xd8, 0x12, 0x8e, 0xad, 0x9b, 0x7d);

interface IuEyeImageFormat : public IUnknown
{
	/*!	 
	 * \brief	Get the image format list
     * \param 	pListFormats	Pointer to image format list	
	 * \param	uiSize			Size of image format list
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ImageFormat_GetList)(void* pListFormats, UINT uiSize) = 0;

	/*!	 
	 * \brief	Get the number of list elements
     * \param 	puiNumber		Number of list elements 		
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ImageFormat_GetNumberOfEntries)(UINT* puiNumber) = 0;

	/*!	 
	 * \brief	Set the ID of the the image format
     * \param 	uiFormatID		ID of the the image format		
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ImageFormat_SetFormat)(UINT uiFormatID) = 0;

	/*!	 
	 * \brief	Get the arbitratry AOI supported value
     * \param 	puiSupported	Get the supported value		
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ImageFormat_GetArbitraryAOISupported)(UINT* puiSupported) = 0;

};


// ============================================================================
/*! \defgroup IuEyeColorConverter uEye image format Interface
* 
*  Proprietary interface for controlling uEye color converter features exposed by 
*  the capture filter. 
*  Allows a DirectShow based program to control and query the device feature feature 
*  related parameters that are not accessible via direct show functions.
* \{
*/
// ============================================================================

// {3D777E6A-0F3A-4925-BA62-01BEAECAF7A0}
DEFINE_GUID(IID_IuEyeColorConverter, 
0x3d777e6a, 0xf3a, 0x4925, 0xba, 0x62, 0x1, 0xbe, 0xae, 0xca, 0xf7, 0xa0);

interface IuEyeColorConverter : public IUnknown
{
	/*!	 
	 * \brief	Set the color converter mode			
	 * \param   iConvertMode	convert mode
	 *
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ColorConverter_SetMode)(INT iConvertMode) = 0;

	/*!	 
	 * \brief	Get the current color converter mode
     * \param 	piConvertMode	convert mode			
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ColorConverter_GetCurrentMode)(INT* piConvertMode) = 0;

	/*!	 
	 * \brief	Get the default color converter mode
     * \param 	piConvertMode	convert mode			
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ColorConverter_GetDefaultMode)(INT* piConvertMode) = 0;

	/*!	 
	 * \brief	Get the supported color converter modes
     * \param 	piConvertMode	convert mode			
	 * 
     * \return	HRESULT			0 on success, error code otherwise.
	 */
	STDMETHOD (ColorConverter_GetSupportedModes)(INT* piConvertMode) = 0;

};


// ============================================================================
/*! \ uEye defines

*/
// ============================================================================

#ifndef __IDS_HEADER__  
// ----------------------------------------------------------------------------
// Hotpixel correction
// ----------------------------------------------------------------------------
#	define IS_HOTPIXEL_DISABLE_CORRECTION                  0x0000
#	define IS_HOTPIXEL_ENABLE_SENSOR_CORRECTION            0x0001
#	define IS_HOTPIXEL_ENABLE_CAMERA_CORRECTION            0x0002
#	define IS_HOTPIXEL_ENABLE_SOFTWARE_USER_CORRECTION     0x0004
#	define IS_HOTPIXEL_DISABLE_SENSOR_CORRECTION           0x0008

#	define IS_HOTPIXEL_GET_CORRECTION_MODE                 0x8000
#	define IS_HOTPIXEL_GET_SUPPORTED_CORRECTION_MODES      0x8001

#	define IS_HOTPIXEL_GET_SOFTWARE_USER_LIST_EXISTS       0x8100
#	define IS_HOTPIXEL_GET_SOFTWARE_USER_LIST_NUMBER       0x8101
#	define IS_HOTPIXEL_GET_SOFTWARE_USER_LIST              0x8102
#	define IS_HOTPIXEL_SET_SOFTWARE_USER_LIST              0x8103
#	define IS_HOTPIXEL_SAVE_SOFTWARE_USER_LIST             0x8104
#	define IS_HOTPIXEL_LOAD_SOFTWARE_USER_LIST             0x8105

#	define IS_HOTPIXEL_GET_CAMERA_FACTORY_LIST_EXISTS      0x8106
#	define IS_HOTPIXEL_GET_CAMERA_FACTORY_LIST_NUMBER      0x8107
#	define IS_HOTPIXEL_GET_CAMERA_FACTORY_LIST             0x8108

#	define IS_HOTPIXEL_GET_CAMERA_USER_LIST_EXISTS         0x8109
#	define IS_HOTPIXEL_GET_CAMERA_USER_LIST_NUMBER         0x810A
#	define IS_HOTPIXEL_GET_CAMERA_USER_LIST                0x810B
#	define IS_HOTPIXEL_SET_CAMERA_USER_LIST                0x810C
#	define IS_HOTPIXEL_GET_CAMERA_USER_LIST_MAX_NUMBER     0x810D
#	define IS_HOTPIXEL_DELETE_CAMERA_USER_LIST             0x810E

#	define IS_HOTPIXEL_GET_MERGED_CAMERA_LIST_NUMBER       0x810F
#	define IS_HOTPIXEL_GET_MERGED_CAMERA_LIST              0x8110

#	define IS_HOTPIXEL_SAVE_SOFTWARE_USER_LIST_UNICODE     0x8111
#	define IS_HOTPIXEL_LOAD_SOFTWARE_USER_LIST_UNICODE     0x8112
#endif
/*!
 * \}
 */	// end of uEye defines

#endif  // #ifndef _UEYE_CAPTURE_INTERFACE_
