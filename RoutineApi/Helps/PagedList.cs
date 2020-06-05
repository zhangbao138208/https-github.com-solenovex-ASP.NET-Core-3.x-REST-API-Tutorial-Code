using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoutineApi.Helps
{
    public class PagedList<T>:List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public PagedList(List<T>items,int count,int currentPage,int pageSize)
        {
            TotalPages = (int)Math.Ceiling(count/(double)pageSize);
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items);
        }

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source,int pageSize,int currentPage)
        {
            int total = await source.CountAsync();
            var item = await source.Skip(pageSize * (currentPage - 1)).Take(pageSize).ToListAsync();
            return new PagedList<T>(item,total,currentPage,pageSize);
        }
    }
}
