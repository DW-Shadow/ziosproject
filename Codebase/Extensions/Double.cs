﻿using System;
using System.Collections.Generic;
using System.Linq;
namespace Zios{ 
    public static class DoubleExtension{
		//=====================
		// Numeric
		//=====================
		public static double MoveTowards(this double current,double end,double speed){
			if(current > end){speed *= -1;}
			current += speed;
			current = end < current ? Math.Max(current,end) : Math.Min(current,end);
			if((speed > 0 && current > end) || (speed < 0 && current < end)){current = end;}
			return current;
		}
		public static double Distance(this double current,double end){
			return Math.Abs(current-end);
		}
	    public static bool Between(this double current,double start,double end){
		    return current >= start && current <= end;
	    }
	    public static bool InRange(this double current,double start,double end){
		    return current.Between(start,end);
	    }
	    public static bool ToBool(this double current){
		    return current != 0;
	    }
	    public static double RoundClosestDown(this double current,params double[] values){
		    double highest = -1;
		    foreach(double value in values){
			    if(current >= value){
				    highest = value;
				    break;
			    }
		    }
		    foreach(double value in values){
			    if(current >= value && value > highest){
				    highest = value;
			    }
		    }
		    return highest;
	    }
	    public static double RoundClosestUp(this double current,params double[] values){
		    double lowest = -1;
		    foreach(double value in values){
			    if(current >= value){
				    lowest = value;
				    break;
			    }
		    }
		    foreach(double value in values){
			    if(current <= value && value < lowest){
				    lowest = value;
			    }
		    }
		    return lowest;
	    }
		public static double Mean(this IEnumerable<double> current){return (double)current.Average();}
		public static double Median(this IEnumerable<double> current){
			int count = current.Cast<object>().Count(); 
			var sorted = current.OrderBy(n=>n); 
			double midValue = sorted.ElementAt(count/2);
			double median = midValue;
			if(count%2==0){
				median = (midValue + sorted.ElementAt((count/2)-1))/2;
			}
			return median;
		}
		public static double Mode(this IEnumerable<double> current){
			return current.GroupBy(x=>x).OrderByDescending(x=>x.Count()).Select(x=>x.Key).FirstOrDefault();
		}
    }
}