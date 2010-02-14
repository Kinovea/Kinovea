//
// This file will be proposed for inclusion in AForge.NET (http://www.aforgenet.com).
//
// Copyright © Joan Charmant, 2010.
// 
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace AForge.Imaging
{
	/// <summary>
    /// Template matching using normalized cross correlation.
    /// </summary>
    /// 
    /// <remarks><para>The class implements template matching using normalized cross correlation
    /// as the similiraty evaluation function.
    /// It performs a complete scan of region of interest in source image.</para>
    /// 
    /// <para>The class processes only grayscale 8 bpp and color 24 bpp images.</para>
    /// 
    /// <para>Sample usage:</para>
    /// <code>
    /// // create template matching algorithm's instance
    /// CorrelationTemplateMatching tm = new CorrelationTemplateMatching( 0.5f );
    /// // find all matchings with similarity above specified.
    /// TemplateMatch[] matchings = tm.ProcessImage( sourceImage, templateImage );
    /// // highlight found matchings
    /// BitmapData data = sourceImage.LockBits(
    ///     new Rectangle( 0, 0, sourceImage.Width, sourceImage.Height ),
    ///     ImageLockMode.ReadWrite, sourceImage.PixelFormat );
    /// foreach ( TemplateMatch m in matchings )
    /// {
    ///     Drawing.Rectangle( data, m.Rectangle, Color.White );
    ///     // do something else with matching
    /// }
    /// sourceImage.UnlockBits( data );
    /// </code>
    /// </remarks>
	public class CorrelationTemplateMatching : ITemplateMatching
	{
		#region Members and Properties
		private float similarityThreshold = 0.5f;
		/// <summary>
        /// Similarity threshold, [0..1].
        /// </summary>
        /// 
        /// <remarks><para>The property sets the minimal acceptable similarity between template
        /// and potential found candidate. If similarity is lower than this value,
        /// then object is not treated as matching with template.
        /// </para>
        /// 
        /// <para>Default value is set to <b>0.5</b>.</para>
        /// </remarks>
		public float SimilarityThreshold
        {
            get { return similarityThreshold; }
            set { similarityThreshold = System.Math.Min( 1, System.Math.Max( 0, value ) ); }
        }
		#endregion
		
		#region Constructors
		/// <summary>
        /// Initializes a new instance of the <see cref="CorrelationTemplateMatching"/> class.
        /// </summary> 
		public CorrelationTemplateMatching() { }
		
		/// <summary>
        /// Initializes a new instance of the <see cref="ExhaustiveTemplateMatching"/> class.
        /// </summary>
        /// 
        /// <param name="similarityThreshold">Similarity threshold.</param>
        /// 
		public CorrelationTemplateMatching( float similarityThreshold )
        {
            this.similarityThreshold = similarityThreshold;
        }
		#endregion
		
		#region Public methods
		public TemplateMatch[] ProcessImage( Bitmap image, Bitmap template )
        {
            return ProcessImage( image, template, new Rectangle( 0, 0, image.Width, image.Height ) );
        }		
		public TemplateMatch[] ProcessImage( BitmapData imageData, BitmapData templateData )
        {
			return ProcessImage( new UnmanagedImage( imageData ), new UnmanagedImage( templateData ),
                new Rectangle( 0, 0, imageData.Width, imageData.Height ) );
		}
		public TemplateMatch[] ProcessImage( UnmanagedImage image, UnmanagedImage template )
        {
            return ProcessImage( image, template, new Rectangle( 0, 0, image.Width, image.Height ) );
        }
		#endregion
		
		#region Implementation of ITemplateMatching
		public TemplateMatch[] ProcessImage( Bitmap image, Bitmap template, Rectangle searchZone )
        {
			// lock source and template images
            BitmapData imageData = image.LockBits(
                new Rectangle( 0, 0, image.Width, image.Height ),
                ImageLockMode.ReadOnly, image.PixelFormat );
			
            BitmapData templateData = template.LockBits(
                new Rectangle( 0, 0, template.Width, template.Height ),
                ImageLockMode.ReadOnly, template.PixelFormat );

            TemplateMatch[] matchings;

            try
            {
                // process the image
                matchings = ProcessImage(
                    new UnmanagedImage( imageData ),
                    new UnmanagedImage( templateData ),
                    searchZone );
            }
            finally
            {
                // unlock images
                image.UnlockBits( imageData );
                template.UnlockBits( templateData );
            }
            
            return matchings;
		}
		public TemplateMatch[] ProcessImage( BitmapData imageData, BitmapData templateData, Rectangle searchZone )
        {
            return ProcessImage( new UnmanagedImage( imageData ), new UnmanagedImage( templateData ), searchZone );
        }
		public TemplateMatch[] ProcessImage( UnmanagedImage image, UnmanagedImage template, Rectangle searchZone )
        {
			// Normalized Cross Correlation.
			
			// check image format
            if (
                ( ( image.PixelFormat != PixelFormat.Format8bppIndexed ) && ( image.PixelFormat != PixelFormat.Format24bppRgb ) ) ||
                ( image.PixelFormat != template.PixelFormat ) )
            {
                throw new UnsupportedImageFormatException( "Unsupported pixel format of the source or template image." );
            }
			
			// clip search zone
            Rectangle zone = searchZone;
            zone.Intersect( new Rectangle( 0, 0, image.Width, image.Height ) );
			
            // search zone's starting point
            int startX = zone.X;
            int startY = zone.Y;
            
            // get source and template image size
            int sourceWidth    = zone.Width;
            int sourceHeight   = zone.Height;
            int templateWidth  = template.Width;
            int templateHeight = template.Height;
            
            // check template's size
            if ( ( templateWidth > sourceWidth ) || ( templateHeight > sourceHeight ) )
            {
                throw new InvalidImagePropertiesException( "Template's size should be smaller or equal to search zone." );
            }
            
            int pixelSize = ( image.PixelFormat == PixelFormat.Format8bppIndexed ) ? 1 : 3;
            int sourceStride = image.Stride;
            
            // Similarity map. 
            // Its size is increased by 4 in both dimensions to increase performance of non-maximum suppresion.
            int mapWidth  = sourceWidth - templateWidth + 1;
            int mapHeight = sourceHeight - templateHeight + 1;
            float[,] map = new float[mapHeight + 4, mapWidth + 4];
            
            // width of template in bytes
            int templateWidthInBytes = templateWidth * pixelSize;
			
            // Algorithm:
			// 1. compute mean of template.
			// 2. for each candidate in the search zone:
			// 	2.a. Compute mean of candidate.
			// 	2.b. Compute correlation coefficient.
			// 3. Suppress non-maxima and sort by correlation coeff.
			
			unsafe
            {
				// 1. compute mean of template.
				byte* baseTpl = (byte*) template.ImageData.ToPointer( );
				byte* tpl = baseTpl;
				int templateOffset = template.Stride - templateWidthInBytes;
				int iTemplateTotal = 0;
				
				// for each row of the template
                for ( int i = 0; i < templateHeight; i++ )
                {
                    // for each pixel of the template
                    for ( int j = 0; j < templateWidthInBytes; j++, tpl++ )
                    {
                    	iTemplateTotal += *tpl; 
                    }
                    tpl += templateOffset;
                }
			
                double fMeanTemplate = (double)iTemplateTotal / (double)(templateHeight * templateWidthInBytes);
                
                // 2. Compute correlation coeff for each candidate in the search zone

				byte* baseSrc = (byte*) image.ImageData.ToPointer( );
                int sourceOffset = image.Stride - templateWidth * pixelSize;
                
                // for each row of the search zone
                for ( int y = 0; y < mapHeight; y++ )
                {
                    // for each pixel of the search zone
                    for ( int x = 0; x < mapWidth; x++ )
                    {
                    	 byte* src = baseSrc + sourceStride * ( y + startY ) + pixelSize * ( x + startX );	
                    	 
						// 2.a. Compute mean of candidate. 
						int iCandidateTotal = 0;
						
						// for each row of the candidate
						for ( int i = 0; i < templateHeight; i++ )
		                {
		                    // for each pixel of the candidate
		                    for ( int j = 0; j < templateWidthInBytes; j++, src++ )
		                    {
		                    	iCandidateTotal += *src; 
		                    }
		                    src += sourceOffset;
		                }
						
						double fMeanCandidate = (double)iCandidateTotal / (double)(templateHeight * templateWidthInBytes);
                
						// 2.b Denominator.
						tpl = baseTpl;
						src = baseSrc + sourceStride * ( y + startY ) + pixelSize * ( x + startX );	
						
						double sit = 0;
						double sisq = 0;
						double stsq = 0;
						
						// for each row of the candidate
						for ( int i = 0; i < templateHeight; i++ )
		                {
		                    // for each pixel of the candidate
		                    for ( int j = 0; j < templateWidthInBytes; j++, tpl++, src++ )
		                    {
		                    	sit += ((*src - fMeanCandidate) * (*tpl - fMeanTemplate));
		                    	sisq += ((*src - fMeanCandidate) * (*src - fMeanCandidate));
		                    	stsq += ((*tpl - fMeanTemplate) * (*tpl - fMeanTemplate));
		                    }
		                    src += sourceOffset;
		                    tpl += templateOffset;
		                }
						
						// Correlation coeff for candidate x,y.
						// Map has a 2-pixel black border for non local-maxima suppression. 
						map[y + 2, x + 2] = 0;
						if(sisq != 0 && stsq != 0)
						{
							// Add to map, filter out lower-than-threshold results.
							float fCorrCoeff = (float)(sit / System.Math.Sqrt(sisq*stsq));
							if( fCorrCoeff >= similarityThreshold )
							{
								map[y + 2, x + 2] = fCorrCoeff;
							}
						}
                    }
                }
			}
			
			// Reparse all results to suppress those that are not local maxima.
			List<TemplateMatch> matchingsList = new List<TemplateMatch>( );
					
			// for each row of the result map.
			for ( int y = 2, maxY = mapHeight + 2; y < maxY; y++ )
            {
                // for each pixel of the result map.
               	for ( int x = 2, maxX = mapWidth + 2; x < maxX; x++ )
                {
               		float currentValue = map[y, x];
               		
                	// Look for a better match in the 5² surrounding window.
					for ( int i = -2; ( currentValue != 0 ) && ( i <= 2 ); i++ )
                    {
                        for ( int j = -2; j <= 2; j++ )
                        {
                            if ( map[y + i, x + j] > currentValue )
                            {
                                currentValue = 0;
                                break;
                            }
                        }
                    }
					
					// Save if really interesting.
                    if ( currentValue != 0 )
                    {
                        matchingsList.Add( new TemplateMatch(
                            new Rectangle( x - 2 + startX, y - 2 + startY, templateWidth, templateHeight ),
                            currentValue) );
                    }
                }
            }
	
            // convert list to array
            TemplateMatch[] matchings = new TemplateMatch[matchingsList.Count];
            matchingsList.CopyTo( matchings );
            
            // sort in descending order
            Array.Sort( matchings, new MatchingsSorter( ) );
            
			return matchings;
		}
		#endregion
				
		// Sorter of found matchings
		// Todo: factorize with ExhaustiveTemplateMatching?
        private class MatchingsSorter : System.Collections.IComparer
        {
            public int Compare( Object x, Object y )
            {
                float diff = ( (TemplateMatch) y ).Similarity - ( (TemplateMatch) x ).Similarity;

                return ( diff > 0 ) ? 1 : ( diff < 0 ) ? -1 : 0;
            }
        }
	}
}
