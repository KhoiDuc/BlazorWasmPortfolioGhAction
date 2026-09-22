import axios from 'axios';

export default {
    async getPosts() {
        try {
            const response = await axios.get('/community/posts');
            return response.data;
        } catch (error) {
            console.error('Error fetching posts:', error);
            return [];
        }
    },

    async savePost(post) {
        try {
            const response = await axios.post('/community/posts', post);
            return response.data;
        } catch (error) {
            console.error('Error saving post:', error);
            throw error;
        }
    },

    async deletePost(postId) {
        try {
            await axios.delete(`/community/posts?id=${postId}`);
        } catch (error) {
            console.error('Error deleting post:', error);
            throw error;
        }
    },

    async updatePost(postId, content) {
        try {
            const response = await axios.put(`/community/posts?id=${postId}`, { content });
            return response.data;
        } catch (error) {
            console.error('Error updating post:', error);
            throw error;
        }
    },

    async getComments(postId) {
        try {
            const response = await axios.get(`/community/comments?post_id=${postId}`);
            return response.data;
        } catch (error) {
            console.error(`Error fetching comments for post ${postId}:`, error);
            return [];
        }
    },

    async addComment(comment) {
        try {
            const response = await axios.post('/community/comments', comment);
            return response.data;
        } catch (error) {
            console.error('Error adding comment:', error);
            throw error;
        }
    },

    async deleteComment(commentId) {
        try {
            await axios.delete(`/community/comments?id=${commentId}`);
        } catch (error) {
            console.error('Error deleting comment:', error);
            throw error;
        }
    },

    async updateComment(commentId, content) {
        try {
            const response = await axios.put(`/community/comments?id=${commentId}`, { content });
            return response.data;
        } catch (error) {
            console.error('Error updating comment:', error);
            throw error;
        }
    },

    // Helper to format date relative to now (e.g., "2 hours ago")
    formatTime(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        const now = new Date();
        const seconds = Math.floor((now - date) / 1000);

        let interval = seconds / 31536000;
        if (interval > 1) return Math.floor(interval) + " years ago";
        interval = seconds / 2592000;
        if (interval > 1) return Math.floor(interval) + " months ago";
        interval = seconds / 86400;
        if (interval > 1) return Math.floor(interval) + " days ago";
        interval = seconds / 3600;
        if (interval > 1) return Math.floor(interval) + " hours ago";
        interval = seconds / 60;
        if (interval > 1) return Math.floor(interval) + " minutes ago";
        return Math.floor(seconds) + " seconds ago";
    },

    // Mock like function (frontend only for now, or update API if needed)
    likePost(postId) {
        // In a real app, this would call an API
        console.log(`Liked post ${postId}`);
        return 1;
    }
};
