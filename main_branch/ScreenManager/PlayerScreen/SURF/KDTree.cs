/* 
 * Comment:
 * This is the K-D Tree implementation from Sebastian Nowozin,
 * Taken from Autopano-sift, and adapted to work with OpenSurf classes.
 * I just made the class use floats instead of int for descriptors data.
 *   joan@kinovea.org
 */


/*
 * Copyright (C) 2004 -- Sebastian Nowozin
 *
 * This program is free software released under the GNU General Public
 * License, which is included in this software package (doc/LICENSE).
 */

/* KDTree.cs
 *
 * A vanilla k-d tree implementation.
 *
 * (C) Copyright 2004 -- Sebastian Nowozin (nowozin@cs.tu-berlin.de)
 *
 * Based on "An introductory tutorial on kd-trees" by Andrew W. Moore,
 * available at http://www.ri.cmu.edu/pubs/pub_2818.html
 */

using System;
using System.Collections;
using System.Collections.Generic;

/* SortedLimitedList replacement by Eric Engle
 *
 * Changes:
 * Modified Add(), and implemented Set to handle node setting semantics
 * not provided by the .NET library.
 *
 * Performance notes:
 * This routine uses a simple insertion sort to handle calls to Add.
 * Each element is compared from right to left. If obj is smaller than the
 * current array object, that object is slid to the right.  Otherwise the hole
 * from the last slide operation is used to hold obj.
 *   Most of the calls to Add() will return -1, i.e. in the normal case only a
 * fraction of the items will be smaller than the largest item on the list.
 * This common case is recognized in the first comparison.  Iteration occurs
 * only for those items that belong on the list, so for the normal case this
 * operation is faster than its strictly linear performance would suggest.
 */

public class SortedLimitedList : ArrayList
{
	private SortedLimitedList ()
	{
	}

	int max;

	public SortedLimitedList (int maxElements)
		: base (maxElements)
	{
		max = maxElements;
	}

	//	Processes list from right to left, sliding each node that is greater
	//	than 'this' to the right.  The loop ends when either the first node is
	//	reached, meaning obj is a new minimum, or it's proper sorted position
	//	in the list is reached.
	//	Returns position of obj or -1 if obj was not placed.

	public override int Add (object obj)
	{
		int pos = Count;

		while (pos > 0 && ((IComparable)base[pos-1]).CompareTo (obj) >= 0) {
			if (pos < max) {
				Set (pos, base[pos-1]);
			}
			pos --;
		}

		if (pos < max) {
			Set (pos, obj);
		} else {
			pos = -1;
		}

		return pos;
	}

	// Sets the argument index to the argument object.
	// Replaces the node if it already exists,
	// adds a new node if at the end of the list,
	// does nothing otherwise.
	internal void Set (int idx, object obj)
	{
		if (idx < Count) {
			base[idx] = obj;
		} else if (idx == Count) {
			base.Add (obj);
		}
	}
}


public class KDTree
{
	// The current element
	IKDTreeDomain dr;

	// The splitting dimension for subtrees.
	int splitDim;

	// The left and the right kd-subtree.
	KDTree left;
	KDTree right;


	private KDTree ()
	{
	}

	public class BestEntry : IComparable
	{
		// Distance between this neighbour point and the target point.
		private double dist;
		public double Distance {
			get {
				return (dist);
			}
			set {
				dist = value;
			}
		}

		private float distSq;
		public float DistanceSq {
			get {
				return (distSq);
			}
			set {
				distSq = value;
			}
		}

		// The neighbour.
		IKDTreeDomain neighbour;
		public IKDTreeDomain Neighbour {
			get {
				return (neighbour);
			}
		}

		public static new bool Equals (object obj1, object obj2)
		{
			BestEntry be1 = (BestEntry) obj1;
			BestEntry be2 = (BestEntry) obj2;

			return (be1.Neighbour == be2.Neighbour);
		}

		private BestEntry ()
		{
		}

		internal BestEntry (IKDTreeDomain neighbour, float distSq, bool squared)
		{
			this.neighbour = neighbour;
			this.distSq = distSq;
		}

		internal BestEntry (IKDTreeDomain neighbour, double dist)
		{
			this.neighbour = neighbour;
			this.dist = dist;
		}

		public int CompareTo (object obj)
		{
			BestEntry be = (BestEntry) obj;

			if (distSq < be.distSq)
				return (-1);
			else if (distSq > be.distSq)
				return (1);

			return (0);
		}
	}

	internal class HREntry : IComparable
	{
		double dist;
		internal double Distance {
			get {
				return (dist);
			}
		}

		HyperRectangle rect;
		internal HyperRectangle HR {
			get {
				return (rect);
			}
		}
		IKDTreeDomain pivot;
		internal IKDTreeDomain Pivot {
			get {
				return (pivot);
			}
		}

		KDTree tree;
		internal KDTree Tree {
			get {
				return (tree);
			}
		}

		private HREntry ()
		{
		}

		internal HREntry (HyperRectangle rect, KDTree tree, IKDTreeDomain pivot,
			double dist)
		{
			this.rect = rect;
			this.tree = tree;
			this.pivot = pivot;
			this.dist = dist;
		}

		public int CompareTo (object obj)
		{
			HREntry hre = (HREntry) obj;

			if (dist < hre.dist)
				return (-1);
			else if (dist > hre.dist)
				return (1);

			return (0);
		}
	}

	internal class HyperRectangle : ICloneable
	{
		float[] leftTop;
		float[] rightBottom;
		
		int dim;

		private HyperRectangle ()
		{
		}

		private HyperRectangle (int dim)
		{
			this.dim = dim;
			leftTop = new float[dim];
			rightBottom = new float[dim];
		}

		public object Clone ()
		{
			HyperRectangle rec = new HyperRectangle (dim);

			for (int n = 0 ; n < dim ; ++n) {
				rec.leftTop[n] = leftTop[n];
				rec.rightBottom[n] = rightBottom[n];
			}

			return (rec);
		}

		static internal HyperRectangle CreateUniverseRectangle (int dim)
		{
			HyperRectangle rec = new HyperRectangle (dim);

			for (int n = 0 ; n < dim ; ++n) 
			{
				rec.leftTop[n] = float.MinValue;
				rec.rightBottom[n] = float.MaxValue;
			}

			return (rec);
		}

		internal HyperRectangle SplitAt (int splitDim, float splitVal)
		{
			if (leftTop[splitDim] >= splitVal || rightBottom[splitDim] < splitVal)
				throw (new ArgumentException ("SplitAt with splitpoint outside rec"));

			HyperRectangle r2 = (HyperRectangle) this.Clone ();
			rightBottom[splitDim] = splitVal;
			r2.leftTop[splitDim] = splitVal;

			return (r2);
		}

		internal bool IsIn (IKDTreeDomain target)
		{
			if (target.DimensionCount != dim)
				throw (new ArgumentException ("IsIn dimension mismatch"));

			for (int n = 0 ; n < dim ; ++n) 
			{
				float targD = target.GetDimensionElement (n);

				if (targD < leftTop[n] || targD >= rightBottom[n])
					return (false);
			}

			return (true);
		}

		// Return true if any part of this HR is reachable from target by no
		// more than 'distRad', false otherwise.
		// The algorithm is specified in the kd-tree paper mentioned at the
		// top of this file, in section 6-7. But there is a mistake in the
		// third distinct case, which should read "hrMax" instead of "hrMin".
		internal bool IsInReach (IKDTreeDomain target, double distRad)
		{
			return (Distance (target) < distRad);
		}

		// Return the distance from the nearest point from within the HR to the target point.
		internal double Distance (IKDTreeDomain target)
		{
			float closestPointN;
			float distance = 0;

			// first compute the closest point within hr to the target. if
			// this point is within reach of target, then there is an
			// intersection between the hypersphere around target with radius
			// 'dist' and this hyperrectangle.
			for (int n = 0 ; n < dim ; ++n) 
			{
				float tI = target.GetDimensionElement(n);
				float hrMin = leftTop[n];
				float hrMax = rightBottom[n];

				closestPointN = 0;
				if (tI <= hrMin) {
					closestPointN = hrMin;
				} else if (tI > hrMin && tI < hrMax) {
					closestPointN = tI;
				} else if (tI >= hrMax) {
					closestPointN = hrMax;
				}

				float dimDist = tI - closestPointN;
				distance += dimDist * dimDist;
			}

			return Math.Sqrt ((double) distance);
		}
	}

	// Find the nearest neighbour to the hyperspace point 'target' within the
	// kd-tree. After return 'resDist' contains the absolute distance from the
	// target point. The nearest neighbour is returned.
	public IKDTreeDomain NearestNeighbour (IKDTreeDomain target, out double resDist)
	{
		HyperRectangle hr = HyperRectangle.CreateUniverseRectangle (target.DimensionCount);

		IKDTreeDomain nearest = NearestNeighbourI (target, hr, Double.PositiveInfinity, out resDist);
		resDist = Math.Sqrt (resDist);

		return (nearest);
	}


	// Internal recursive algorithm for the kd-tree nearest neighbour search.
	private IKDTreeDomain NearestNeighbourI (IKDTreeDomain target, HyperRectangle hr, double maxDistSq, out double resDistSq)
	{
		resDistSq = Double.PositiveInfinity;

		IKDTreeDomain pivot = dr;

		HyperRectangle leftHr = hr;
		HyperRectangle rightHr = leftHr.SplitAt(splitDim, pivot.GetDimensionElement(splitDim));

		HyperRectangle nearerHr, furtherHr;
		KDTree nearerKd, furtherKd;

		// step 5-7
		if (target.GetDimensionElement (splitDim) <=
			pivot.GetDimensionElement (splitDim))
		{
			nearerKd = left;
			nearerHr = leftHr;
			furtherKd = right;
			furtherHr = rightHr;
		} else {
			nearerKd = right;
			nearerHr = rightHr;
			furtherKd = left;
			furtherHr = leftHr;
		}

		// step 8
		IKDTreeDomain nearest = null;
		double distSq;
		if (nearerKd == null) {
			distSq = Double.PositiveInfinity;
		} else {
			nearest = nearerKd.NearestNeighbourI (target, nearerHr,
				maxDistSq, out distSq);
		}

		// step 9
		maxDistSq = Math.Min (maxDistSq, distSq);

		// step 10
		if (furtherHr.IsInReach (target, Math.Sqrt (maxDistSq))) {
			double ptDistSq = KDTree.DistanceSq (pivot, target);
			if (ptDistSq < distSq) {
				// steps 10.1.1 to 10.1.3
				nearest = pivot;
				distSq = ptDistSq;
				maxDistSq = distSq;
			}

			// step 10.2
			double tempDistSq;
			IKDTreeDomain tempNearest = null;
			if (furtherKd == null) {
				tempDistSq = Double.PositiveInfinity;
			} else {
				tempNearest = furtherKd.NearestNeighbourI (target,
					furtherHr, maxDistSq, out tempDistSq);
			}

			// step 10.3
			if (tempDistSq < distSq) {
				nearest = tempNearest;
				distSq = tempDistSq;
			}
		}

		resDistSq = distSq;
		return (nearest);
	}

	public ArrayList NearestNeighbourList(IKDTreeDomain target, out double resDist, int q)
	{
		HyperRectangle hr =
			HyperRectangle.CreateUniverseRectangle (target.DimensionCount);

		SortedLimitedList best = new SortedLimitedList (q);

		IKDTreeDomain nearest = NearestNeighbourListI (best, q, target, hr,
			Double.PositiveInfinity, out resDist);
		resDist = Math.Sqrt (resDist);

		foreach (BestEntry be in best)
			be.Distance = Math.Sqrt (be.Distance);

		return (best);
	}


	private IKDTreeDomain NearestNeighbourListI (SortedLimitedList best, int q, IKDTreeDomain target, HyperRectangle hr, double maxDistSq, out double resDistSq)
	{
		resDistSq = Double.PositiveInfinity;

		IKDTreeDomain pivot = dr;

		best.Add (new BestEntry (dr, KDTree.DistanceSq (target, dr)));

		HyperRectangle leftHr = hr;
		HyperRectangle rightHr = leftHr.SplitAt (splitDim,
			pivot.GetDimensionElement (splitDim));

		HyperRectangle nearerHr, furtherHr;
		KDTree nearerKd, furtherKd;

		// step 5-7
		if (target.GetDimensionElement (splitDim) <=
			pivot.GetDimensionElement (splitDim))
		{
			nearerKd = left;
			nearerHr = leftHr;
			furtherKd = right;
			furtherHr = rightHr;
		} else {
			nearerKd = right;
			nearerHr = rightHr;
			furtherKd = left;
			furtherHr = leftHr;
		}

		// step 8
		IKDTreeDomain nearest = null;
		double distSq;

		// No child, bottom reached!
		if (nearerKd == null) {
			distSq = Double.PositiveInfinity;
		} else {
			nearest = nearerKd.NearestNeighbourListI (best, q, target, nearerHr,
				maxDistSq, out distSq);
		}

		// step 9
		//maxDistSq = Math.Min (maxDistSq, distSq);
		if (best.Count >= q)
			maxDistSq = ((BestEntry) best[q - 1]).Distance;
		else
			maxDistSq = Double.PositiveInfinity;

		// step 10
		if (furtherHr.IsInReach (target, Math.Sqrt (maxDistSq))) {
			double ptDistSq = KDTree.DistanceSq (pivot, target);
			if (ptDistSq < distSq) {
				// steps 10.1.1 to 10.1.3
				nearest = pivot;
				distSq = ptDistSq;

				// TODO: use k-element list
				/*
				best.Add (new BestEntry (pivot, ptDistSq));
				best.Sort ();
				*/

				maxDistSq = distSq;
			}

			// step 10.2
			double tempDistSq;
			IKDTreeDomain tempNearest = null;
			if (furtherKd == null) {
				tempDistSq = Double.PositiveInfinity;
			} else {
				tempNearest = furtherKd.NearestNeighbourListI (best, q, target,
					furtherHr, maxDistSq, out tempDistSq);
			}

			// step 10.3
			if (tempDistSq < distSq) {
				nearest = tempNearest;
				distSq = tempDistSq;
			}
		}

		resDistSq = distSq;
		return (nearest);
	}
	
	// Limited Best-Bin-First k-d-tree nearest neighbour search.
	//
	// (Using the algorithm described in the paper "Shape indexing using
	// approximate nearest-neighbour search in high-dimensional spaces",
	// available at http://www.cs.ubc.ca/spider/lowe/papers/cvpr97-abs.html)
	//
	// Find the approximate nearest neighbour to the hyperspace point 'target'
	// within the kd-tree using 'searchSteps' tail recursions at most (each
	// recursion deciding about one neighbours' fitness).
	//
	// After return 'resDist' contains the absolute distance of the
	// approximate nearest neighbour from the target point. The approximate
	// nearest neighbour is returned.
	public ArrayList NearestNeighbourListBBF (IKDTreeDomain target, int q, int searchSteps)
	{
		HyperRectangle hr = HyperRectangle.CreateUniverseRectangle (target.DimensionCount);

		SortedLimitedList best = new SortedLimitedList (q);
		SortedLimitedList searchHr = new SortedLimitedList (searchSteps);

		float dummyDist;
		IKDTreeDomain nearest = NearestNeighbourListBBFI (best, q, target, hr, float.MaxValue, out dummyDist, searchHr, ref searchSteps);

		foreach (BestEntry be in best)
			be.Distance = Math.Sqrt (be.DistanceSq);

		return (best);
	}
	
	public IKDTreeDomain NearestNeighbourListBBF(IKDTreeDomain target, int searchSteps)
	{
		// Same as above but always return the NN only.
		ArrayList list = NearestNeighbourListBBF(target, 1, searchSteps);
		return ((BestEntry)list[0]).Neighbour;
	}


	private IKDTreeDomain NearestNeighbourListBBFI (SortedLimitedList best,
		int q, IKDTreeDomain target, HyperRectangle hr, float maxDistSq,
		out float resDistSq, SortedLimitedList searchHr, ref int searchSteps)
	{
		resDistSq = float.MaxValue;

		IKDTreeDomain pivot = dr;

		best.Add (new BestEntry(dr, KDTree.DistanceSq(target, dr), true));

		HyperRectangle leftHr = hr;
		HyperRectangle rightHr = leftHr.SplitAt (splitDim,
			pivot.GetDimensionElement (splitDim));

		HyperRectangle nearerHr, furtherHr;
		KDTree nearerKd, furtherKd;

		// step 5-7
		if (target.GetDimensionElement (splitDim) <= pivot.GetDimensionElement (splitDim))
		{
			nearerKd = left;
			nearerHr = leftHr;
			furtherKd = right;
			furtherHr = rightHr;
		} 
		else 
		{
			nearerKd = right;
			nearerHr = rightHr;
			furtherKd = left;
			furtherHr = leftHr;
		}

		// step 8
		IKDTreeDomain nearest = null;
		float distSq;

		searchHr.Add (new HREntry (furtherHr, furtherKd, pivot, furtherHr.Distance (target)));

		// No child, bottom reached!
		if (nearerKd == null) 
		{
			distSq = float.MaxValue;
		} 
		else 
		{
			nearest = nearerKd.NearestNeighbourListBBFI (best, q, target, nearerHr, maxDistSq, out distSq, searchHr, ref searchSteps);
		}

		// step 9
		if (best.Count >= q) 
		{
			maxDistSq = ((BestEntry)best[q - 1]).DistanceSq;
		} 
		else
			maxDistSq = float.MaxValue;

		if (searchHr.Count > 0) 
		{
			HREntry hre = (HREntry) searchHr[0];
			searchHr.RemoveAt (0);

			furtherHr = hre.HR;
			furtherKd = hre.Tree;
			pivot = hre.Pivot;
		}

		// step 10
		searchSteps -= 1;
		if (searchSteps > 0 && furtherHr.IsInReach (target, Math.Sqrt (maxDistSq)))
		{
			float ptDistSq = KDTree.DistanceSq (pivot, target);
			if (ptDistSq < distSq) {
				// steps 10.1.1 to 10.1.3
				nearest = pivot;
				distSq = ptDistSq;

				maxDistSq = distSq;
			}

			// step 10.2
			float tempDistSq;
			IKDTreeDomain tempNearest = null;
			if (furtherKd == null) {
				tempDistSq = float.MaxValue;
			} else {
				tempNearest = furtherKd.NearestNeighbourListBBFI (best, q, target, furtherHr, maxDistSq, out tempDistSq, searchHr,
					ref searchSteps);
			}

			// step 10.3
			if (tempDistSq < distSq) {
				nearest = tempNearest;
				distSq = tempDistSq;
			}
		}

		resDistSq = distSq;
		return (nearest);
	}


	public static float DistanceSq(IKDTreeDomain t1, IKDTreeDomain t2)
	{
		float distance = 0;

		for (int n = 0 ; n < t1.DimensionCount ; ++n) 
		{
			float dimDist = t1.GetDimensionElement(n) - t2.GetDimensionElement(n);
			distance += dimDist * dimDist;
		}

		return distance;
	}

	static private IKDTreeDomain GoodCandidate (ArrayList exset, out int splitDim)
	{
		IKDTreeDomain first = exset[0] as IKDTreeDomain;
		if (first == null) {
			Console.WriteLine ("type: {0}", exset[0]);
			throw (new Exception ("Not of type IKDTreeDomain (TODO: custom exception)"));
		}

		int dim = first.DimensionCount;

		// initialize temporary hr search min/max values
		double[] minHr = new double[dim];
		double[] maxHr = new double[dim];
		for (int k = 0 ; k < dim ; ++k) {
			minHr[k] = Double.PositiveInfinity;
			maxHr[k] = Double.NegativeInfinity;
		}

		foreach (IKDTreeDomain dom in exset) {
			for (int k = 0 ; k < dim ; ++k) {
				double dimE = dom.GetDimensionElement (k);

				if (dimE < minHr[k])
					minHr[k] = dimE;
				if (dimE > maxHr[k])
					maxHr[k] = dimE;
			}
		}

		// find the maximum range dimension
		double[] diffHr = new double[dim];
		int maxDiffDim = 0;
		double maxDiff = 0.0;
		for (int k = 0 ; k < dim ; ++k) {
			diffHr[k] = maxHr[k] - minHr[k];
			if (diffHr[k] > maxDiff) {
				maxDiff = diffHr[k];
				maxDiffDim = k;
			}
		}

		// the splitting dimension is maxDiffDim
		// now find a exemplar as close to the arithmetic middle as possible
		double middle = (maxDiff / 2.0) + minHr[maxDiffDim];
		IKDTreeDomain exemplar = null;
		double exemMinDiff = Double.PositiveInfinity;

		foreach (IKDTreeDomain dom in exset) {
			double curDiff = Math.Abs (dom.GetDimensionElement (maxDiffDim) - middle);
			if (curDiff < exemMinDiff) {
				exemMinDiff = curDiff;
				exemplar = dom;
			}
		}

		// return the values
		splitDim = maxDiffDim;

		return (exemplar);
	}

	// Build a kd-tree from a list of elements. All elements must implement
	// the IKDTreeDomain interface and must have the same dimensionality.
	static public KDTree CreateKDTree (ArrayList exset)
	{
		if (exset.Count == 0)
			return (null);

		KDTree cur = new KDTree ();
		cur.dr = GoodCandidate (exset, out cur.splitDim);

		ArrayList leftElems = new ArrayList ();
		ArrayList rightElems = new ArrayList ();

		// split the exemplar set into left/right elements relative to the
		// splitting dimension
		double bound = cur.dr.GetDimensionElement (cur.splitDim);
		foreach (IKDTreeDomain dom in exset) 
		{
			// ignore the current element
			if (dom == cur.dr)
				continue;

			if (dom.GetDimensionElement (cur.splitDim) <= bound) {
				leftElems.Add (dom);
			} else {
				rightElems.Add (dom);
			}
		}

		// recurse
		cur.left = KDTree.CreateKDTree (leftElems);
		cur.right = KDTree.CreateKDTree (rightElems);

		return (cur);
	}

	// The interface to be implemented by all data elements within the
	// kd-tree. As every element is represented in domain space by a
	// multidimensional vector, the interface provides readonly methods to
	// access into this vector and to get its dimension.
	public interface IKDTreeDomain
	{
		int DimensionCount {
			get;
		}
		
		float GetDimensionElement(int dim);
	}
}


