using PavolsBookStore.Models.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PavolsBookStore.Models.DataLayer
{
  public class QueryOptions<T>
  {
    public Expression<Func<T,Object>> OrderBy { get; set; }
    public string OrderByDirection { get; set; } = "asc";
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    private string[] includes;
    public string Includes
    {
      set => includes = value.Replace(" ", "").Split(',');
    }

    public string[] GetIncludes() => includes ?? new string[0];

    public WhereClauses<T> WhereClauses { get; set; }
    public Expression<Func<T,bool>> Where
    {
      set
      {
        if (WhereClauses == null)
          WhereClauses = new WhereClauses<T>();

        WhereClauses.Add(value);
      }
    }

    public bool HasWhere => WhereClauses != null;
    public bool HasOrderBy => OrderBy != null;
    public bool HasPaging => PageNumber > 0 && PageSize > 0;
  }

  public class WhereClauses<T> : List<Expression<Func<T, bool>>> { } 
}

/*
var options = new QueryOptions<Book>
{
  Include = "BookAuthors.Author, Genre",
  WhereClauses = new WhereClauses <Book>{
      {b => b.GenreId == "novel" },
      {b => b.Price < 10
  }
}

List<Expression<Func<Book, bool>>>
*/
