#region License
/*
Copyright © Joan Charmant 2009.
joan.charmant@gmail.com 

*/
#endregion
using System;

namespace OpenSURF
{
	/// <summary>
	/// Match. 
	/// A simple class to link two Interest Points together.
	/// </summary>
	public class Match
	{

		#region Properties
		public Ipoint Ipt1
		{
			get { return m_ipt1; }
		}
		public Ipoint Ipt2
		{
			get { return m_ipt2; }
		}
		public double Distance1
		{
			get { return m_fDistance1; }
			set { m_fDistance1 = value; }
		}
		public double Distance2
		{
			get { return m_fDistance2; }
			set { m_fDistance2 = value; }
		}
		#endregion
		
		#region Members
		private Ipoint m_ipt1;
		private Ipoint m_ipt2;
		private double m_fDistance1;
		private double m_fDistance2;
		#endregion
		
		public Match()
		{
		}
		public Match(Ipoint _ipt1, Ipoint _ipt2)
		{
			m_ipt1 = _ipt1;
			m_ipt2 = _ipt2;
		}
		public Match(Ipoint _ipt1, Ipoint _ipt2, double _fDistance1, double _fDistance2)
		{
			m_ipt1 = _ipt1;
			m_ipt2 = _ipt2;
			m_fDistance1 = _fDistance1;
			m_fDistance2 = _fDistance2;
		}
		
	}
}
