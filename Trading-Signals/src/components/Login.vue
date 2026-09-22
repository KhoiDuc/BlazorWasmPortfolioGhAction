<template>
  <div class="container mt-5">
    <div class="row justify-content-center">
      <div class="col-md-6">
        <div class="card">
          <div class="card-body">
            <div class="text-center mb-4">
              <img src="../assets/logo.png" alt="Logo" class="img-fluid" style="max-height: 100px;">
            </div>
            <h4 class="text-center mb-4">Login</h4>
           <div v-if="isLoading" class="text-center"><div class="spinner"></div></div>
           <form v-else @submit.prevent="handleSubmit">
             <div class="mb-3">
               <label for="email" class="form-label">Email address</label>
               <input type="email" class="form-control" id="email" v-model="email" required>
             </div>
             <div class="mb-3">
               <label for="password" class="form-label">Password</label>
               <input type="password" class="form-control" id="password" v-model="password" required>
             </div>
             <div class="mb-3 d-flex align-items-center">
               <input type="checkbox" class="form-check-input" id="rememberMe" style="margin-right: 10px;">
               <label class="form-check-label" for="rememberMe">Remember me</label>
             </div>
             <div v-if="errorMessage" class="alert alert-danger" role="alert">
               {{ errorMessage }}
             </div>
             <button type="submit" class="btn btn-primary w-100 mb-3">Login</button>
             <button type="button" class="btn btn-secondary w-100" @click="handleDemoLogin">Demo Login (Test Mode)</button>
           </form>
         </div>
       </div>
     </div>
   </div>
 </div>
</template>

<script>
import { ref } from 'vue';
import { useRouter } from 'vue-router';

export default {
  name: 'LoginPage',
  emits: ['close-login'],
  setup(props, { emit }) {
    const email = ref('');
    const password = ref('');
    const errorMessage = ref('');
    const router = useRouter();
    const isLoading = ref(false);
    const handleSubmit = async () => {
     isLoading.value = true;
      errorMessage.value = ''; // Clear any previous error messages

      if (!email.value) {
        errorMessage.value = 'Please enter your email address.';
       isLoading.value = false;
        return;
      }

      if (!password.value) {
        errorMessage.value = 'Please enter your password.';
       isLoading.value = false;
        return;
      }

      try {
        const response = await fetch('/dnse-auth-service/login', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            username: email.value,
            password: password.value,
          }),
        });

        const data = await response.json();

        if (response.ok && data.token) { // Check for HTTP status and accessToken
          // Store the token and user info in local storage
          localStorage.setItem('token', data.token);
          localStorage.setItem('refreshToken', data.refreshToken);
          localStorage.setItem('userInfo', JSON.stringify(data)); // Store the entire data object
          // Emit the close-login event
          emit('close-login');
          // Redirect to the my-portfolio page
          router.push('/my-portfolio');
        } else {
          // Handle API errors (e.g., invalid credentials)
          errorMessage.value = data.message || data.error || 'Invalid credentials. Please try again.';
        }
      } catch (error) {
        // Handle network errors
        errorMessage.value = 'An error occurred. Please check your network connection.';
      } finally {
       isLoading.value = false;
      }
    };

    const handleDemoLogin = () => {
      const demoUser = {
        name: 'Demo User',
        custodyCode: 'DEMO123',
        id: 'demo-user-id',
        email: 'demo@example.com'
      };
      const demoToken = 'demo-token-' + Date.now();
      
      localStorage.setItem('token', demoToken);
      localStorage.setItem('refreshToken', 'demo-refresh-token');
      localStorage.setItem('userInfo', JSON.stringify(demoUser));
      
      emit('close-login');
      router.push('/my-portfolio');
    };

    return {
      email,
      password,
      handleSubmit,
      handleDemoLogin,
      errorMessage
    };
  },
};
</script>

<style scoped>
.container {
  background-image: url('https://images.unsplash.com/photo-1625580794409-4577d3b77557'); /* Stock bullish image */
  background-size: cover;
  background-position: center;
  min-height: 100vh; /* Ensure the background covers the entire viewport height */
}

.card {
  background-color: rgba(255, 255, 255, 0.8); /* Semi-transparent white */
  border: 1px solid rgba(0, 0, 0, 0.1);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.card-header h4 {
    color: #333;
}
.form-label{
    color:#333;
}

.form-control {
  border-radius: 20px;
  border: 1px solid rgba(0, 0, 0, 0.1);
  transition: border-color 0.2s ease-in-out;
}

.form-control:focus {
  border-color: #007bff; /* Or any other color you prefer */
  box-shadow: 0 0 0 0.2rem rgba(0, 123, 255, 0.25);
}

.btn-primary {
  background-color: #007bff;
  border-color: #007bff;
  border-radius: 20px;
  padding: 10px 20px;
  transition: background-color 0.2s ease-in-out;
}

.btn-primary:hover {
  background-color: #0056b3;
  border-color: #0056b3;
}

.form-check-label {
    color:#333
}

a {
  color: #007bff;
  text-decoration: none;
}

a:hover {
  text-decoration: underline;
}

/* Add component-specific styles here if needed */

.spinner {
  border: 4px solid rgba(0, 0, 0, 0.1);
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border-left-color: #09f;
  animation: spin 1s ease infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
