using backend.Dtos.Comments;
using backend.Exceptions;
using backend.Models;
using backend.Repository.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services.Implementation
{
    public class CommentService : ICommentService
    {
        private const int MaxHtmlLength = 10000;

        private readonly ICommentRepository _commentRepository;
        private readonly IWorkRepository _workRepository;

        public CommentService(ICommentRepository commentRepository, IWorkRepository workRepository)
        {
            _commentRepository = commentRepository;
            _workRepository = workRepository;
        }

        public async Task<List<CommentResponse>> GetAsync(Guid userId, Guid workId)
        {
            await EnsureWorkOwnedAsync(userId, workId);
            var comments = await _commentRepository.GetByWorkAsync(workId);
            return comments.Select(ToResponse).ToList();
        }

        public async Task<CommentResponse> CreateAsync(Guid userId, Guid workId, CommentRequest request)
        {
            await EnsureWorkOwnedAsync(userId, workId);
            var (html, plainText) = Clean(request.Text);

            var comment = new WorkComment
            {
                Id = Guid.NewGuid(),
                Text = html,
                PlainText = plainText,
                CreatedAt = DateTime.UtcNow,
                WorkId = workId,
                AuthorId = userId,
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();

            return ToResponse(comment);
        }

        public async Task<CommentResponse> UpdateAsync(Guid userId, Guid workId, Guid commentId, CommentRequest request)
        {
            await EnsureWorkOwnedAsync(userId, workId);
            var comment = await GetCommentAsync(workId, commentId);
            var (html, plainText) = Clean(request.Text);

            comment.Text = html;
            comment.PlainText = plainText;
            comment.UpdatedAt = DateTime.UtcNow;
            await _commentRepository.SaveChangesAsync();

            return ToResponse(comment);
        }

        public async Task DeleteAsync(Guid userId, Guid workId, Guid commentId)
        {
            await EnsureWorkOwnedAsync(userId, workId);
            var comment = await GetCommentAsync(workId, commentId);

            _commentRepository.Remove(comment);
            await _commentRepository.SaveChangesAsync();
        }

        private async Task EnsureWorkOwnedAsync(Guid userId, Guid workId)
        {
            var work = await _workRepository.GetByIdAsync(workId, userId);
            if (work is null)
            {
                throw new NotFoundException(ErrorCode.WorkNotFound);
            }
        }

        private async Task<WorkComment> GetCommentAsync(Guid workId, Guid commentId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId, workId);
            return comment ?? throw new NotFoundException(ErrorCode.CommentNotFound);
        }

        private static (string Html, string PlainText) Clean(string text)
        {
            var (html, plainText) = CommentHtmlSanitizer.Sanitize(text);

            if (plainText.Length == 0)
            {
                throw new BadRequestException(ErrorCode.CommentEmpty);
            }
            if (html.Length > MaxHtmlLength)
            {
                throw new BadRequestException(ErrorCode.CommentTooLong);
            }

            return (html, plainText);
        }

        private static CommentResponse ToResponse(WorkComment comment) => new()
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
        };
    }
}
