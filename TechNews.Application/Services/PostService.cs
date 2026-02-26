using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using TechNews.Application.DTOs;
using TechNews.Application.Interfaces;
using TechNews.Domain.Entities;
using TechNews.Domain.Enums;
using TechNews.Domain.Interfaces;

namespace TechNews.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Post> _postRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Tag> _tagRepo;
        private readonly IRepository<PostTag> _postTagRepo;
        private readonly IRepository<PostRevision> _revisionRepo;
        private readonly IWebHostEnvironment _env;

        public PostService(
            IUnitOfWork unitOfWork,
            IRepository<Post> postRepo,
            IRepository<Category> categoryRepo,
            IRepository<Tag> tagRepo,
            IRepository<PostTag> postTagRepo,
            IRepository<PostRevision> revisionRepo,
            IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _postRepo = postRepo;
            _categoryRepo = categoryRepo;
            _tagRepo = tagRepo;
            _postTagRepo = postTagRepo;
            _revisionRepo = revisionRepo;
            _env = env;
        }

        public async Task<IEnumerable<object>> GetAllPostsAsync(string? userId = null, bool isAdmin = false)
        {
            var posts = await _postRepo.GetAllAsync(p => p.Category);
            var query = posts.Where(p => !p.IsDeleted);
            
            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.AuthorId == userId);
            }

            return query.Select(p => new {
                p.Id,
                p.Title,
                p.Slug,
                CategoryName = p.Category?.Name,
                p.CreatedDate,
                p.Status,
                p.Thumbnail
            }).OrderByDescending(p => p.CreatedDate);
        }

        public async Task<object?> GetPostByIdAsync(int id, string? userId = null, bool isAdmin = false)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) return null;

            if (!isAdmin && !string.IsNullOrEmpty(userId) && post.AuthorId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập bài viết này.");

            var category = await _categoryRepo.GetByIdAsync(post.CategoryId);

            return new {
                post.Id,
                post.Title,
                post.ShortDescription,
                post.Content,
                post.CategoryId,
                CategoryName = category?.Name,
                post.Status,
                post.MetaTitle,
                post.MetaDescription,
                post.Tags,
                post.Thumbnail,
                ThumbnailUrl = post.Thumbnail 
            };
        }

        public async Task CreatePostAsync(CreatePostDto dto, string authorId)
        {
            var post = new Post
            {
                Title = dto.Title,
                Slug = GenerateSlug(dto.Title),
                ShortDescription = dto.ShortDescription,
                Content = dto.Content,
                CategoryId = dto.CategoryId,
                Status = dto.Status,
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                Tags = dto.Tags,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                ViewCount = 0,
                AuthorId = authorId
            };

            if (dto.ThumbnailFile != null)
            {
                post.Thumbnail = await UploadImageAsync(dto.ThumbnailFile);
            }
            else if (!string.IsNullOrEmpty(dto.ThumbnailUrl))
            {
                post.Thumbnail = dto.ThumbnailUrl;
            }

            await _postRepo.AddAsync(post);
            await _unitOfWork.CompleteAsync();

            await SyncTagsAsync(post.Id, dto.Tags);
        }

        public async Task UpdatePostAsync(int id, EditPostDto dto, string? userId = null, bool isAdmin = false)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) throw new KeyNotFoundException("Post not found");

            if (!isAdmin && !string.IsNullOrEmpty(userId) && post.AuthorId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền sửa bài viết này.");

            await CreateRevisionAsync(post);

            post.Title = dto.Title;
            post.Slug = GenerateSlug(dto.Title);
            post.ShortDescription = dto.ShortDescription;
            post.Content = dto.Content;
            post.CategoryId = dto.CategoryId;
            post.Status = dto.Status;
            post.MetaTitle = dto.MetaTitle;
            post.MetaDescription = dto.MetaDescription;
            post.Tags = dto.Tags;
            post.ModifiedDate = DateTime.Now;

            if (dto.ThumbnailFile != null)
            {
                post.Thumbnail = await UploadImageAsync(dto.ThumbnailFile);
            }
            else if (!string.IsNullOrEmpty(dto.ThumbnailUrl))
            {
                post.Thumbnail = dto.ThumbnailUrl;
            }

            await _postRepo.UpdateAsync(post);
            await _unitOfWork.CompleteAsync();

            await SyncTagsAsync(post.Id, dto.Tags);
        }

        public async Task DeletePostAsync(int id, string? userId = null, bool isAdmin = false)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) throw new KeyNotFoundException("Post not found");

            if (!isAdmin && !string.IsNullOrEmpty(userId) && post.AuthorId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa bài viết này.");

            post.IsDeleted = true;
            await _postRepo.UpdateAsync(post);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<bool> RestorePostAsync(int id, string? userId = null, bool isAdmin = false)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) return false;

            if (!isAdmin && !string.IsNullOrEmpty(userId) && post.AuthorId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền khôi phục bài viết này.");

            post.IsDeleted = false;
            await _postRepo.UpdateAsync(post);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        public async Task PermanentDeletePostAsync(int id, string? userId = null, bool isAdmin = false)
        {
            var post = await _postRepo.GetByIdAsync(id);
            if (post == null) throw new KeyNotFoundException("Post not found");

            if (!isAdmin && !string.IsNullOrEmpty(userId) && post.AuthorId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa vĩnh viễn bài viết này.");

            var postTags = await _postTagRepo.FindAsync(pt => pt.PostId == id);
            foreach(var pt in postTags)
            {
               await _postTagRepo.DeleteAsync(pt);
            }
            
            var revisions = await _revisionRepo.FindAsync(r => r.PostId == id);
             foreach(var rev in revisions)
            {
               await _revisionRepo.DeleteAsync(rev);
            }

            await _postRepo.DeleteAsync(post);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IEnumerable<object>> GetTrashAsync(string? userId = null, bool isAdmin = false)
        {
            var posts = await _postRepo.GetAllAsync(p => p.Category);
            var query = posts.Where(p => p.IsDeleted);
            
            if (!isAdmin && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(p => p.AuthorId == userId);
            }

            return query.Select(p => new {
                p.Id,
                p.Title,
                p.Slug,
                CategoryName = p.Category?.Name,
                p.CreatedDate,
                p.Status,
                p.Thumbnail
            }).OrderByDescending(p => p.CreatedDate);
        }

        public async Task<IEnumerable<object>> GetRevisionsAsync(int postId)
        {
            var revisions = await _revisionRepo.FindAsync(r => r.PostId == postId);
            return revisions.OrderByDescending(r => r.Version)
                            .Select(r => new { r.Id, r.Version, r.ModifiedDate, r.Title });
        }

        public async Task RestoreRevisionAsync(int revisionId, string? userId = null, bool isAdmin = false)
        {
            var revision = await _revisionRepo.GetByIdAsync(revisionId);
            if (revision == null) throw new KeyNotFoundException("Revision not found");

            var post = await _postRepo.GetByIdAsync(revision.PostId);
            if (post == null) throw new KeyNotFoundException("Post not found");

            if (!isAdmin && !string.IsNullOrEmpty(userId) && post.AuthorId != userId)
                throw new UnauthorizedAccessException("Bạn không có quyền khôi phục phiên bản của bài viết này.");

            await CreateRevisionAsync(post);

            post.Title = revision.Title;
            post.ShortDescription = revision.ShortDescription;
            post.Content = revision.Content;
            post.ModifiedDate = DateTime.Now;

            await _postRepo.UpdateAsync(post);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return string.Empty;

            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "content");
            uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "thumbnails");
            
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/thumbnails/" + uniqueFileName;
        }


        private string GenerateSlug(string title)
        {
            return title.ToLower().Replace(" ", "-").Replace("đ", "d"); 
        }

        private async Task CreateRevisionAsync(Post post)
        {
            var revisions = await _revisionRepo.FindAsync(r => r.PostId == post.Id);
            int startVersion = 1;
            if (revisions.Any())
            {
                startVersion = revisions.Max(r => r.Version) + 1;
            }

            var revision = new PostRevision
            {
                PostId = post.Id,
                Title = post.Title,
                ShortDescription = post.ShortDescription,
                Content = post.Content,
                ModifiedDate = DateTime.Now,
                Version = startVersion
            };
            await _revisionRepo.AddAsync(revision);
            await _unitOfWork.CompleteAsync();
        }

        private async Task SyncTagsAsync(int postId, string tags)
        {
            if (string.IsNullOrWhiteSpace(tags)) 
            {
                var allLinks = await _postTagRepo.FindAsync(pt => pt.PostId == postId);
                foreach(var link in allLinks) await _postTagRepo.DeleteAsync(link);
                await _unitOfWork.CompleteAsync();
                return;
            }

            var existingLinks = await _postTagRepo.FindAsync(pt => pt.PostId == postId);
            foreach(var link in existingLinks) await _postTagRepo.DeleteAsync(link);
            await _unitOfWork.CompleteAsync();

            var tagNames = tags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).Distinct();
            foreach (var tagName in tagNames)
            {
                var slug = GenerateSlug(tagName);
                
                var existingTags = await _tagRepo.FindAsync(t => t.Slug == slug);
                var tag = existingTags.FirstOrDefault();

                if (tag == null)
                {
                    tag = new Tag { Name = tagName, Slug = slug, Count = 0 };
                    await _tagRepo.AddAsync(tag);
                    await _unitOfWork.CompleteAsync();
                }

                await _postTagRepo.AddAsync(new PostTag { PostId = postId, TagId = tag.Id });
            }
            await _unitOfWork.CompleteAsync();
        }
    }
}