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
        private readonly IWorkAccessService _access;
        private readonly IUserRepository _userRepository;
        private readonly IRealtimeOutbox _outbox;

        public CommentService(
            ICommentRepository commentRepository,
            IWorkAccessService access,
            IUserRepository userRepository,
            IRealtimeOutbox outbox)
        {
            _outbox = outbox;
            _commentRepository = commentRepository;
            _access = access;
            _userRepository = userRepository;
        }

        public async Task<List<CommentResponse>> GetAsync(Guid userId, Guid workId)
        {
            var access = await _access.RequireAsync(userId, workId, WorkAccessLevel.Read);
            var comments = await _commentRepository.GetByWorkAsync(workId);
            return comments.Select(c => ToResponse(c, userId, access.Role)).ToList();
        }

        public async Task<CommentResponse> CreateAsync(Guid userId, Guid workId, CommentRequest request)
        {
            var access = await _access.RequireAsync(userId, workId, WorkAccessLevel.Edit);
            var (html, plainText) = Clean(request.Text);

            var author = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException(ErrorCode.WorkNotFound);

            var comment = new WorkComment
            {
                Id = Guid.NewGuid(),
                Text = html,
                PlainText = plainText,
                CreatedAt = DateTime.UtcNow,
                WorkId = workId,
                AuthorId = userId,
                Author = author,
            };

            await _commentRepository.AddAsync(comment);
            await _commentRepository.SaveChangesAsync();
            _outbox.Enqueue(n => n.CommentsChangedAsync(workId));

            return ToResponse(comment, userId, access.Role);
        }

        public async Task<CommentResponse> UpdateAsync(Guid userId, Guid workId, Guid commentId, CommentRequest request)
        {
            var access = await _access.RequireAsync(userId, workId, WorkAccessLevel.Edit);
            var comment = await GetCommentAsync(workId, commentId);
            if (comment.AuthorId != userId)
            {
                throw new ForbiddenException(ErrorCode.WorkAccessDenied);
            }

            var (html, plainText) = Clean(request.Text);

            comment.Text = html;
            comment.PlainText = plainText;
            comment.UpdatedAt = DateTime.UtcNow;
            await _commentRepository.SaveChangesAsync();
            _outbox.Enqueue(n => n.CommentsChangedAsync(workId));

            return ToResponse(comment, userId, access.Role);
        }

        public async Task DeleteAsync(Guid userId, Guid workId, Guid commentId)
        {
            var access = await _access.RequireAsync(userId, workId, WorkAccessLevel.Edit);
            var comment = await GetCommentAsync(workId, commentId);
            if (comment.AuthorId != userId && access.Role != WorkRole.Owner)
            {
                throw new ForbiddenException(ErrorCode.WorkAccessDenied);
            }

            _commentRepository.Remove(comment);
            await _commentRepository.SaveChangesAsync();
            _outbox.Enqueue(n => n.CommentsChangedAsync(workId));
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

        private static CommentResponse ToResponse(WorkComment comment, Guid userId, WorkRole role) => new()
        {
            Id = comment.Id,
            Text = comment.Text,
            AuthorId = comment.AuthorId,
            AuthorName = comment.Author.DisplayName,
            AuthorUsername = comment.Author.Username,
            AuthorAvatarVersion = comment.Author.AvatarVersion,
            CanEdit = comment.AuthorId == userId && role != WorkRole.Viewer,
            CanDelete = role != WorkRole.Viewer && (comment.AuthorId == userId || role == WorkRole.Owner),
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
        };
    }
}
