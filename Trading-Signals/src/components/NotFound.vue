<template>
  <div class="page-wrapper">
    <NavBar />
    <div class="container mt-4 flex-grow-1">
      <div class="error-container">
        <div class="error-content">
          <h1 class="error-code">404</h1>
          <h2 class="error-title">Page Not Found</h2>
          <p class="error-message">
            Oops! The page you're looking for doesn't exist.
          </p>
          <p class="error-submessage">
            The URL may be misspelled or the page has been moved.
          </p>
          <div class="error-actions">
            <router-link to="/" class="btn btn-primary btn-lg me-3">
              <i class="bi bi-house-door"></i> Go to Home
            </router-link>
            <button @click="goBack" class="btn btn-outline-secondary btn-lg">
              <i class="bi bi-arrow-left"></i> Go Back
            </button>
          </div>
          
          <!-- Quick Links -->
          <div class="quick-links mt-5">
            <h5>Quick Links:</h5>
            <div class="links-grid">
              <router-link to="/" class="link-item">
                <i class="bi bi-house-door"></i> Home
              </router-link>
              <router-link to="/crypto" class="link-item">
                <i class="bi bi-currency-bitcoin"></i> Crypto
              </router-link>
              <router-link to="/futures" class="link-item">
                <i class="bi bi-activity"></i> Futures
              </router-link>
              <router-link to="/stock" class="link-item">
                <i class="bi bi-graph-up"></i> Stock
              </router-link>
              <router-link to="/forex" class="link-item">
                <i class="bi bi-cash-coin"></i> Forex
              </router-link>
              <router-link to="/commodities" class="link-item">
                <i class="bi bi-gem"></i> Commodities
              </router-link>
              <router-link to="/real-estate" class="link-item">
                <i class="bi bi-gem"></i> Real Estate
              </router-link>
              <template v-if="isLoggedIn">
                <router-link to="/my-portfolio" class="link-item">
                  <i class="bi bi-briefcase"></i> Portfolio
                </router-link>
                <router-link to="/community" class="link-item">
                  <i class="bi bi-people"></i> Community
                </router-link>
              </template>
            </div>
          </div>
        </div>
      </div>
    </div>
    <AppFooter />
  </div>
</template>

<script>
import NavBar from './NavBar.vue';
import AppFooter from './AppFooter.vue';

export default {
  name: 'NotFound',
  components: {
    NavBar,
    AppFooter,
  },
  computed: {
    isLoggedIn() {
      return !!localStorage.getItem('token');
    }
  },
  methods: {
    goBack() {
      if (window.history.length > 2) {
        this.$router.go(-1);
      } else {
        this.$router.push('/');
      }
    },
  },
};
</script>

<style scoped>
.page-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

.flex-grow-1 {
  flex: 1;
}

.error-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
  padding: 2rem 0;
}

.error-content {
  text-align: center;
  max-width: 600px;
}

.error-code {
  font-size: 10rem;
  font-weight: 900;
  color: #0d6efd;
  margin: 0;
  line-height: 1;
  text-shadow: 4px 4px 8px rgba(0, 0, 0, 0.1);
  animation: bounce 2s ease-in-out infinite;
}

@keyframes bounce {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-20px);
  }
}

.error-title {
  font-size: 2.5rem;
  font-weight: 700;
  color: #212529;
  margin: 1rem 0;
}

.error-message {
  font-size: 1.25rem;
  color: #6c757d;
  margin: 1rem 0;
}

.error-submessage {
  font-size: 1rem;
  color: #adb5bd;
  margin-bottom: 2rem;
}

.error-actions {
  display: flex;
  justify-content: center;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
  margin-top: 2rem;
}

.error-actions .btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  transition: all 0.3s ease;
}

.error-actions .btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.quick-links {
  margin-top: 3rem;
  padding-top: 2rem;
  border-top: 2px solid #e9ecef;
}

.quick-links h5 {
  color: #495057;
  margin-bottom: 1.5rem;
  font-weight: 600;
}

.links-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 1rem;
  max-width: 500px;
  margin: 0 auto;
}

.link-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  background: #f8f9fa;
  border-radius: 8px;
  text-decoration: none;
  color: #495057;
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.link-item:hover {
  background: #e9ecef;
  color: #0d6efd;
  border-color: #0d6efd;
  transform: translateY(-3px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.link-item i {
  font-size: 1.5rem;
}

/* Responsive Design */
@media (max-width: 768px) {
  .error-code {
    font-size: 7rem;
  }

  .error-title {
    font-size: 2rem;
  }

  .error-message {
    font-size: 1.1rem;
  }

  .error-actions {
    flex-direction: column;
  }

  .error-actions .btn {
    width: 100%;
    max-width: 300px;
  }
  
  .error-actions .me-3 {
    margin-right: 0 !important;
  }

  .links-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 480px) {
  .error-code {
    font-size: 5rem;
  }

  .error-title {
    font-size: 1.5rem;
  }

  .error-message {
    font-size: 1rem;
  }
  
  .error-container {
    min-height: 50vh;
  }
}
</style>
