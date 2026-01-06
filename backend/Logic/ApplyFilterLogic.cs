using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApplication.Models;

namespace Logic {
    public static class ApplyFilterLogic {
        public static IEnumerable<T> ApplyFilter<T>(this IEnumerable<T> source, FilterOptions filter, Func<T, string, object?> retrieveEntityName) {
            IOrderedEnumerable<T>? orderedSource = null;

            if (filter.Where != null)
                source = WhereFilter(source, filter.Where, retrieveEntityName);

            if (filter.Sort != null) {
                foreach (var sortItem in filter.Sort.Split(",")) {
                    bool isDescent = sortItem.StartsWith("-");
                    string proprietyName = (sortItem.StartsWith("+") || sortItem.StartsWith("-")) ? sortItem.Substring(1) : sortItem;

                    if (orderedSource == null) {
                        if (!isDescent) {
                            orderedSource = source.OrderBy(v => retrieveEntityName(v, proprietyName));
                        } else {
                            orderedSource = source.OrderByDescending(v => retrieveEntityName(v, proprietyName));
                        }
                    } else {
                        if (!isDescent) {
                            orderedSource = orderedSource.ThenBy(v => retrieveEntityName(v, proprietyName));
                        } else {
                            orderedSource = orderedSource.ThenByDescending(v => retrieveEntityName(v, proprietyName));
                        }
                    }
                }
            }

            if (orderedSource == null) {
                orderedSource = source.Order();
            }

            return orderedSource.Skip(filter.Page * filter.Limit).Take(filter.Limit);
        }

        public static IQueryable<T> ApplyFilterForCount<T>(this IQueryable<T> source, FilterOptions filter, Func<T, string, object?> retrieveEntityName)  {
            if (filter.Where != null)
                source = WhereFilter(source, filter.Where, retrieveEntityName);

            return source;
        }

        public static IEnumerable<T> ApplyFilterForCount<T>(this IEnumerable<T> source, FilterOptions filter, Func<T, string, object?> retrieveEntityName) {
            if (filter.Where != null)
                source = WhereFilter(source, filter.Where, retrieveEntityName);

            return source;
        }

        private static IEnumerable<T> WhereFilter<T>(IEnumerable<T> source, string whereString, Func<T, string, object?> retrieveEntityName) {
            foreach (var where in whereString.Split(";")) {
                string key = where.Split("#")[0];
                string whereValue = where.Split("#")[1];

                foreach (var single in whereValue.Split("|")) {
                    switch (single[0]) {
                        case '[': {
                            List<int> inList = single.Length > 2 ? single.Substring(1, single.Length - 2).Split(",").Select(str => int.Parse(str)).ToList() : [];

                            source = source.Where(v => inList.Contains((UInt16) retrieveEntityName(v, key)!));
                            break;
                        }

                        case '<':
                        case '>': {
                            bool equal = single[1] == '=';
                            string value = single.Substring(equal ? 2 : 1);

                            source = source.Where(v => equal ? (v.Equals(retrieveEntityName(v, key))) : (single[0] == '<' ? (value.CompareTo(retrieveEntityName(v, key)) > 0) : (value.CompareTo(retrieveEntityName(v, key)) < 0)));
                            break;
                        }

                        case '=': {
                            string value = single.Substring(1);

                            source = source.Where(v => value.Equals(retrieveEntityName(v, key)));
                            break;
                        }
                    }
                }

            }

            return source;
        }

        private static IQueryable<T> WhereFilter<T>(IQueryable<T> source, string whereString, Func<T, string, object?> retrieveEntityName) {
            foreach (var where in whereString.Split(";")) {
                string key = where.Split("#")[0];
                string whereValue = where.Split("#")[1];

                foreach (var single in whereValue.Split("|")) {
                    switch (single[0]) {
                        case '[': {
                            List<int> inList = single.Substring(1, single.Length - 2).Split(",").Select(str => int.Parse(str)).ToList();

                            source = source.Where(v => inList.Contains((int)retrieveEntityName(v, key)!));
                            break;
                        }

                        case '<':
                        case '>': {
                            bool equal = single[1] == '=';
                            string value = single.Substring(equal ? 2 : 1);

                            source = source.Where(v => equal ? (v.Equals(retrieveEntityName(v, key))) : (single[0] == '<' ? (value.CompareTo(retrieveEntityName(v, key)) > 0) : (value.CompareTo(retrieveEntityName(v, key)) < 0)));
                            break;
                        }

                        case '=': {
                            string value = single.Substring(1);

                            source = source.Where(v => value.Equals(retrieveEntityName(v, key)));
                            break;
                        }
                    }
                }

            }

            return source;
        }
    }
}
