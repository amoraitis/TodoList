namespace TodoList.Web.Models
{
    public class HomeViewModel
    {
        public TodoItem[] RecentlyAddedTodos { get; set; }
        public TodoItem[] CloseDueToTodos { get; set; }
    }
}
