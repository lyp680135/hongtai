namespace Models
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// 用于验证是否登录的 父级Model
    /// </summary>
    [Authorize]
    public class AuthorizeModel : BaseModel<DataLibrary.DataContext>
    {
        public AuthorizeModel(DataLibrary.DataContext db)
             : base(db)
        {
        }
    }
}
