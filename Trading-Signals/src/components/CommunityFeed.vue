<template>
  <div class="community-feed">
    <div v-if="loading" class="text-center py-5">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    
    <div v-else-if="posts.length === 0" class="text-center py-5 text-muted">
      <p>No posts yet. Be the first to share something!</p>
    </div>

    <div v-else class="post-list">
      <div v-for="post in posts" :key="post.id">
        <div v-if="post.content || post.image" class="card mb-3 post-card text-start">
          <div class="card-body">
            <div class="d-flex align-items-center mb-3">
              <div class="avatar-circle me-3">
                {{ getInitials(post.user_name || 'Anonymous') }}
              </div>
              <div>
                <h6 class="mb-0 fw-bold">{{ post.user_name || 'Anonymous' }}</h6>
                <small class="text-muted">{{ formatTime(post.created_at) }}</small>
              </div>
               <div class="ms-auto" v-if="(currentUserId && post.user_id == currentUserId) || (currentUserCode && post.user_code == currentUserCode) || (currentUserInfo && post.user_name === (currentUserInfo.name || currentUserInfo.username)) || post.user_id === 'unknown' || !post.user_id"> <!-- Allow deleting/editing if owner or legacy -->
                  <button class="btn btn-sm btn-link text-secondary text-decoration-none me-1" @click="startEdit(post)" title="Sửa bài viết">
                     <i class="fas fa-edit"></i>
                  </button>
                  <button class="btn btn-sm btn-link text-danger text-decoration-none" @click="deletePost(post.id)" title="Xóa bài viết">
                     <i class="fas fa-trash-alt"></i>
                  </button>
              </div>
            </div>
          
          <div v-if="post.isEditing" class="mb-3">
            <textarea 
              class="form-control mb-2" 
              rows="3" 
              v-model="post.editContent"
              placeholder="Chỉnh sửa nội dung bài viết..."
            ></textarea>
            <div class="d-flex gap-2 justify-content-end">
              <button class="btn btn-sm btn-light rounded-pill px-3" @click="cancelEdit(post)">Hủy</button>
              <button class="btn btn-sm btn-primary rounded-pill px-3" @click="saveEdit(post)" :disabled="!post.editContent.trim()">Lưu</button>
            </div>
          </div>
          <p v-else class="card-text mb-3" style="white-space: pre-wrap;">{{ post.content }}</p>
          
          <div v-if="post.image" class="mb-3">
            <img :src="post.image" class="img-fluid rounded" alt="Post content">
          </div>
          
          <div class="d-flex pt-2 border-top">
            <button 
              class="btn btn-sm btn-light me-3 flex-grow-1"
              @click="likePost(post.id)"
              :class="{ 'text-primary': post.liked }"
            >
              <i class="fas fa-thumbs-up me-1"></i> Like ({{ post.likes || 0 }})
            </button>
            <button class="btn btn-sm btn-light flex-grow-1" @click="toggleComments(post)">
              <i class="fas fa-comment me-1"></i> Comment
            </button>
          </div>
          
          <!-- Comments Section -->
          <div v-if="post.showComments" class="mt-3 pt-3 border-top">
             <div v-if="post.commentsLoading" class="text-center">
                 <div class="spinner-border spinner-border-sm text-secondary" role="status"></div>
             </div>
             <div v-else>
                  <div v-for="comment in post.comments" :key="comment.id" class="d-flex mb-2 align-items-start comment-item">
                      <div class="avatar-circle-sm me-2 flex-shrink-0">
                          {{ getInitials(comment.user_name) }}
                      </div>
                      <div class="bg-light p-2 rounded flex-grow-1 comment-bubble">
                          <div class="d-flex justify-content-between align-items-center mb-1">
                             <span class="fw-bold small me-2">{{ comment.user_name }}</span>
                             <div class="d-flex align-items-center">
                               <small class="text-muted me-2" style="font-size: 0.72rem;">{{ formatTime(comment.created_at) }}</small>
                               <!-- Comment edit/delete action controls -->
                               <div v-if="(currentUserId && comment.user_id == currentUserId) || (currentUserInfo && comment.user_name === (currentUserInfo.name || currentUserInfo.username)) || comment.user_id === 'unknown' || !comment.user_id" class="comment-actions gap-1">
                                  <button class="btn btn-sm btn-link text-secondary text-decoration-none p-0 me-1" @click="startEditComment(comment)" title="Sửa bình luận" style="font-size: 0.75rem; border: none; background: none;">
                                     <i class="fas fa-edit"></i>
                                  </button>
                                  <button class="btn btn-sm btn-link text-danger text-decoration-none p-0" @click="deleteComment(post, comment.id)" title="Xóa bình luận" style="font-size: 0.75rem; border: none; background: none;">
                                     <i class="fas fa-trash-alt"></i>
                                  </button>
                               </div>
                             </div>
                          </div>
                          
                          <!-- Edit Comment Input Field -->
                          <div v-if="comment.isEditing" class="mt-1">
                             <input 
                               type="text" 
                               class="form-control form-control-sm mb-1 py-1 px-2" 
                               v-model="comment.editContent"
                               @keyup.enter="saveEditComment(post, comment)"
                               style="font-size: 0.8rem; border-radius: 8px;"
                             >
                             <div class="d-flex gap-2 justify-content-end">
                               <button class="btn btn-xxs btn-light rounded-pill px-2 py-0 border-0" @click="cancelEditComment(comment)" style="font-size: 0.7rem; background-color: #e2e8f0; color: #475569;">Hủy</button>
                               <button class="btn btn-xxs btn-primary rounded-pill px-2 py-0 border-0" @click="saveEditComment(post, comment)" :disabled="!comment.editContent.trim()" style="font-size: 0.7rem; background-color: #3b82f6; color: white;">Lưu</button>
                             </div>
                          </div>
                          <p v-else class="mb-0 small" style="white-space: pre-wrap; font-size: 0.85rem; color: #cbd5e1;">{{ comment.content }}</p>
                      </div>
                  </div>
                 
                 <!-- Add Comment Input -->
                 <div class="d-flex mt-2 align-items-center">
                     <input 
                        type="text" 
                        class="form-control form-control-sm me-2" 
                        placeholder="Write a comment..." 
                        v-model="post.newComment"
                        @keyup.enter="submitComment(post)"
                     >
                     <button class="btn btn-sm btn-primary" @click="submitComment(post)" :disabled="!post.newComment">
                         <i class="fas fa-paper-plane"></i>
                     </button>
                 </div>
             </div>
          </div>
        </div>
      </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { useNotification } from '@kyvg/vue3-notification';
import communityService from '../services/communityService';

export default {
  name: 'CommunityFeed',
  setup() {
    const { notify } = useNotification();
    const posts = ref([]);
    const loading = ref(true);
    const currentUserId = ref('');
    const currentUserCode = ref('');
    const currentUserInfo = ref({});

    const fetchPosts = async () => {
      loading.value = true;
      try {
        posts.value = await communityService.getPosts();
        console.log("FETCHED POSTS:", posts.value);
        console.log("CURRENT USER INFO:", currentUserInfo.value);
        console.log("CURRENT USER ID:", currentUserId.value);
        console.log("CURRENT USER CODE:", currentUserCode.value);
        
        // Initialize reactive properties for comments and editing
        posts.value.forEach(p => {
            p.showComments = false;
            p.comments = [];
            p.commentsLoading = false;
            p.newComment = '';
            p.isEditing = false;
            p.editContent = '';
        });
      } finally {
        loading.value = false;
      }
    };

    onMounted(() => {
      const userInfoStr = localStorage.getItem('userInfo');
      if (userInfoStr) {
          currentUserInfo.value = JSON.parse(userInfoStr);
          currentUserId.value = String(currentUserInfo.value.id || '');
          currentUserCode.value = String(currentUserInfo.value.custodyCode || '');
      }
      fetchPosts();
    });

    const getInitials = (name) => {
      if (!name) return 'U';
      return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    };

    const formatTime = (timestamp) => {
      return communityService.formatTime(timestamp);
    };

    const likePost = (postId) => {
      const newLikes = communityService.likePost(postId);
      const post = posts.value.find(p => p.id === postId);
      if (post) {
        post.likes = newLikes;
        post.liked = true; // Local state only for visual feedback
      }
    };
    
    const deletePost = async (postId) => {
       if(confirm("Are you sure you want to delete this post?")) {
           await communityService.deletePost(postId);
           fetchPosts();
       }
    };

    const toggleComments = async (post) => {
        post.showComments = !post.showComments;
        if (post.showComments && post.comments.length === 0) {
            post.commentsLoading = true;
            try {
                const fetchedComments = await communityService.getComments(post.id);
                post.comments = (fetchedComments || []).map(c => ({
                    ...c,
                    isEditing: false,
                    editContent: ''
                }));
            } finally {
                post.commentsLoading = false;
            }
        }
    };

    const submitComment = async (post) => {
        if (!post.newComment || !post.newComment.trim()) return;
        
        const commentData = {
            post_id: post.id,
            user_id: currentUserInfo.value.custodyCode || currentUserInfo.value.id || 'unknown',
            user_name: currentUserInfo.value.name || 'Anonymous',
            content: post.newComment
        };

        try {
            const newComment = await communityService.addComment(commentData);
            post.comments.push({
                ...newComment,
                isEditing: false,
                editContent: ''
            });
            post.newComment = '';
        } catch (error) {
            notify({ type: 'error', title: 'Error', text: 'Failed to post comment' });
        }
    };

    const startEdit = (post) => {
        post.isEditing = true;
        post.editContent = post.content;
    };

    const cancelEdit = (post) => {
        post.isEditing = false;
        post.editContent = '';
    };

    const saveEdit = async (post) => {
        if (!post.editContent || !post.editContent.trim()) return;
        try {
            await communityService.updatePost(post.id, post.editContent);
            post.content = post.editContent;
            post.isEditing = false;
        } catch (error) {
            notify({ type: 'error', title: 'Error', text: 'Cập nhật bài viết thất bại.' });
        }
    };

    const startEditComment = (comment) => {
        comment.isEditing = true;
        comment.editContent = comment.content;
    };

    const cancelEditComment = (comment) => {
        comment.isEditing = false;
        comment.editContent = '';
    };

    const saveEditComment = async (post, comment) => {
        if (!comment.editContent || !comment.editContent.trim()) return;
        try {
            await communityService.updateComment(comment.id, comment.editContent);
            comment.content = comment.editContent;
            comment.isEditing = false;
        } catch (error) {
            notify({ type: 'error', title: 'Error', text: 'Cập nhật bình luận thất bại.' });
        }
    };

    const deleteComment = async (post, commentId) => {
        if (confirm("Bạn có chắc chắn muốn xóa bình luận này?")) {
            try {
                await communityService.deleteComment(commentId);
                post.comments = post.comments.filter(c => c.id !== commentId);
            } catch (error) {
                notify({ type: 'error', title: 'Error', text: 'Xóa bình luận thất bại.' });
            }
        }
    };

    return {
      posts,
      loading,
      getInitials,
      formatTime,
      likePost,
      deletePost,
      fetchPosts,
      currentUserId,
      currentUserCode,
      currentUserInfo,
      toggleComments,
      submitComment,
      startEdit,
      cancelEdit,
      saveEdit,
      startEditComment,
      cancelEditComment,
      saveEditComment,
      deleteComment
    };
  }
};
</script>

<style scoped>
.avatar-circle {
  width: 44px;
  height: 44px;
  background: linear-gradient(135deg, #3b82f6 0%, #8b5cf6 100%);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 15px;
  box-shadow: 0 4px 10px rgba(59, 130, 246, 0.25);
  border: 2px solid rgba(255, 255, 255, 0.08);
}

.avatar-circle-sm {
  width: 32px;
  height: 32px;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.8rem;
  box-shadow: 0 2px 6px rgba(16, 185, 129, 0.2);
}

.post-card {
  border: 1px solid rgba(0, 0, 0, 0.06) !important;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.03) !important;
  background: #ffffff;
  color: #1e293b;
  border-radius: 16px !important;
  transition: transform 0.25s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.25s ease;
}

.post-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 35px rgba(59, 130, 246, 0.05) !important;
  border-color: rgba(0, 0, 0, 0.1) !important;
}

.btn-light {
  background-color: rgba(0, 0, 0, 0.02);
  border: 1px solid rgba(0, 0, 0, 0.06);
  color: #475569;
  font-weight: 600;
  border-radius: 30px;
  padding: 8px 16px;
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}

.btn-light:hover {
  background-color: rgba(0, 0, 0, 0.05);
  color: #0f172a;
  border-color: rgba(0, 0, 0, 0.12);
  transform: translateY(-1px);
}

.btn-light:active {
  transform: translateY(0);
}

.text-primary {
  color: #2563eb !important;
}

.btn-light.text-primary {
  background-color: rgba(59, 130, 246, 0.08);
  border-color: rgba(59, 130, 246, 0.2);
  color: #2563eb !important;
}

.form-control {
  background-color: #ffffff;
  color: #0f172a;
  border: 1px solid #cbd5e1;
  border-radius: 12px;
}

.form-control:focus {
  background-color: #ffffff;
  color: #0f172a;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

.form-control-sm {
  border-radius: 30px;
  border: 1px solid #cbd5e1;
  padding: 8px 16px;
  background-color: #ffffff;
  color: #0f172a;
  transition: all 0.2s ease;
}

.form-control-sm:focus {
  background-color: #ffffff;
  color: #0f172a;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.15);
}

.comment-bubble {
  border-radius: 12px !important;
  background-color: #f8fafc !important;
  border: 1px solid #e2e8f0;
  transition: all 0.2s ease;
  color: #334155;
}

.comment-item:hover .comment-bubble {
  background-color: #f1f5f9 !important;
  border-color: #cbd5e1;
}

.comment-actions {
  opacity: 0.5;
  transition: opacity 0.2s ease;
}

.comment-item:hover .comment-actions {
  opacity: 1;
}

.btn-xxs {
  padding: 2px 8px;
  font-size: 0.7rem;
  line-height: 1.2;
}
</style>
