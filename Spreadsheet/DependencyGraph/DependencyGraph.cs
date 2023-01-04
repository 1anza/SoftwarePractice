// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)
// Completed Verison by Alivia Liljenquist

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        private Dictionary<string, HashSet<string>> dependents;
        private Dictionary<string, HashSet<string>> dependees;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependents = new Dictionary<string, HashSet<string>>();
            dependees = new Dictionary<string, HashSet<string>>();
        }


        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// Loops through dependents - for every dependee listed in dependent value, add 1
        /// </summary>
        public int Size
        {
            get 
            {
                int s = 0;
                foreach (KeyValuePair<string, HashSet<string>>kvp in dependents)
                {
                    foreach (string dependee in kvp.Value)
                    {
                        s++;
                    }
                }
                return s;
            }
        }


        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// Gets dependents of a string
        /// </summary>
        public int this[string s]
        {
            get
            {
                HashSet<string> getDependents;
                if (dependees.TryGetValue(s, out getDependents))
                {
                    return getDependents.Count;
                }
                return 0;
            }
        }


        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// Is there anything that depends on s? True or False
        /// </summary>
        public bool HasDependents(string s)
        {
            if (dependents.ContainsKey(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// Is there anything that s depends on? True or False
        /// </summary>
        public bool HasDependees(string s)
        {
            if (dependees.ContainsKey(s))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Enumerates dependents(s).
        /// List of things that depend on s, else return empty
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        {
            HashSet<string> getDependees;
            if (dependents.TryGetValue(s, out getDependees))
            {
                return getDependees;
            }
            else
            {
                return new HashSet<string>();
            }
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// List of things s depends on, else return empty
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            HashSet<string> getDependents;
            if (dependees.TryGetValue(s, out getDependents))
            {
                return getDependents;
            }
            else
            {
                return new HashSet<string>();
            }
        }


        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            if (s == null || t == null)
            {
                throw new ArgumentException("Incorrect inserted values in AddDependency.");
            }

            /// Adds t as something that depends on s in dependents dictionary - "s":{"t"}
            /// If nothing depends on s ...
            if (!HasDependents(s))
            {
                HashSet<string> dep = new HashSet<string>();
                dep.Add(t);
                dependents.Add(s, dep);
            }
            else
            {
                HashSet<string> getDependees;
                if (dependents.TryGetValue(s, out getDependees))
                {
                    if (!getDependees.Contains(t))
                    {
                        getDependees.Add(t);
                    }
                }
            }

            /// Adds s as something t depends on in dependees dictionary - "t":{"s"}
            /// If s does not depend on something...
            if (!HasDependees(t))
            {
                HashSet<string> dep2 = new HashSet<string>();
                dep2.Add(s);
                dependees.Add(t, dep2);
            }
            else
            {
                HashSet<string> getDependents;
                if (dependees.TryGetValue(t, out getDependents))
                {
                    if (!getDependents.Contains(s))
                    {
                        getDependents.Add(s);
                    }
                }
            }
        }


        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            if (s == null || t == null)
            {
                throw new ArgumentException("Incorrect inserted values in RemoveDependency.");
            }
            /// Removes t from s key in dependent dictionary
            HashSet<string> getDependees;
            if (dependents.TryGetValue(s, out getDependees))
            {
                getDependees.Remove(t);
                if (getDependees.Count == 0)
                {
                    dependents.Remove(s);
                }
            }

            /// Removes s from t key in dependee dictionary
            HashSet<string> getDependents;
            if (dependees.TryGetValue(t, out getDependents))
            {
                getDependents.Remove(s);
                if (getDependents.Count == 0)
                {
                    dependees.Remove(t);
                }
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            HashSet<string> getDependees;
            if (dependents.TryGetValue(s, out getDependees))
            {
                string[] oldDdees = getDependees.ToArray();
                
                // Removes s from dependent sets in dependees dictionary
                foreach (string oldDdee in oldDdees)
                {
                    HashSet<string> oldDdeesHash;
                    if (dependees.TryGetValue(oldDdee, out oldDdeesHash))
                    {
                        oldDdeesHash.Remove(s);
                    }
                }

                // Clears dependee set in s dependent dictionary and add things that depend on it
                getDependees.Clear();
                foreach (string newDdent in newDependents)
                {
                    getDependees.Add(newDdent);
                }
            }

            foreach (string newDdent in newDependents)
            {
                AddDependency(s, newDdent);
            }
        }


        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            HashSet<string> getDependents;
            if (dependees.TryGetValue(s, out getDependents))
            {
                string[] prevDependents = getDependents.ToArray();
                getDependents.Clear();

                foreach (string prevDependent in prevDependents)
                {
                    HashSet<string> prevDependentHash;
                    if (dependents.TryGetValue(prevDependent, out prevDependentHash))
                    {
                        prevDependentHash.Remove(s);
                    }
                }

                foreach (string newDependent in newDependees)
                {
                    getDependents.Add(newDependent);
                }
            }

            foreach (string newDependee in newDependees)
            {
                AddDependency(newDependee, s);
            }
        }

    }

}

