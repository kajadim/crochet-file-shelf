using backend.Dtos.Comments;

namespace backend.Services.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentResponse>> GetAsync(Guid userId, Guid workId);
        Task<CommentResponse> CreateAsync(Guid userId, Guid workId, CommentRequest request);
        Task<CommentResponse> UpdateAsync(Guid userId, Guid workId, Guid commentId, CommentRequest request);
        Task DeleteAsync(Guid userId, Guid workId, Guid commentId);
    }
}
