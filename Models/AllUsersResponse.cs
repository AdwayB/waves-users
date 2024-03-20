namespace waves_users.Models;

public class AllUsersResponse {
    public int NumberOfUsers { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int NumberOfPages { get; set; }
    public List<User> Users { get; set; }
    
    public AllUsersResponse(int numberOfUsers, int pageSize, int pageNumber, int numberOfPages, List<User> users) {
        NumberOfUsers = numberOfUsers;
        PageSize = pageSize;
        PageNumber = pageNumber;
        NumberOfPages = numberOfPages;
        Users = users;
    }
}