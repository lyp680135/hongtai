namespace WarrantyApiCenter.Controllers.V1
{
    public class PostData
    {
        private int pageIndex;
        private int pageSize;

        public int PageIndex { get => this.pageIndex; set => this.pageIndex = value; }

        public int PageSize { get => this.pageSize; set => this.pageSize = value; }
    }
}
