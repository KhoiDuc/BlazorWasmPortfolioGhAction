<template>
  <div class="price-alert-widget">
    <!-- Alert Input Section -->
    <div class="alert-input-section card mb-3">
      <div class="card-header bg-warning">
        <h6 class="mb-0">
          <i class="bi bi-bell"></i> Set Price Alert for {{ symbol }}
        </h6>
      </div>
      <div class="card-body">
        <div class="row mb-2">
          <div class="col-md-4">
            <select class="form-select" v-model="operator">
              <option value="<=">≤ Less than or equal</option>
              <option value=">=">≥ Greater than or equal</option>
            </select>
          </div>
          <div class="col-md-8">
            <div class="input-group">
              <span class="input-group-text">$</span>
              <input 
                type="number" 
                class="form-control" 
                v-model="alertPrice"
                :placeholder="`Enter target price for ${symbol}`"
                step="0.01"
              />
              <button 
                class="btn btn-warning" 
                @click="createAlert"
                :disabled="!alertPrice || alertPrice <= 0 || isCreating"
              >
                <span v-if="isCreating" class="spinner-border spinner-border-sm me-1"></span>
                {{ isCreating ? 'Creating...' : 'Set Alert' }}
              </button>
            </div>
          </div>
        </div>
        <small class="text-muted">
          <span v-if="alertPrice && operator === '<='">You'll be notified when price drops to or below <strong>${{ (alertPrice * 1.01).toFixed(2) }}</strong> (alert price + 1%)</span>
          <span v-else-if="alertPrice && operator === '>='">You'll be notified when price rises to or above <strong>${{ (alertPrice * 0.99).toFixed(2) }}</strong> (alert price - 1%)</span>
          <span v-else>Enter a target price and select condition</span>
        </small>
      </div>
    </div>

    <!-- Active Alerts List -->
    <div class="alerts-list" v-if="alerts.length > 0">
      <h6 class="mb-2">
        <i class="bi bi-list-check"></i> Active Alerts ({{ activeAlertsCount }})
      </h6>
      <div class="list-group">
        <div 
          v-for="alert in alerts" 
          :key="`${alert.symbol}-${alert.asset_type}`"
          class="list-group-item d-flex justify-content-between align-items-center"
          :class="{ 'alert-inactive': !alert.is_active }"
        >
          <div>
            <strong>{{ alert.symbol }}</strong>
            <span class="badge bg-secondary ms-2">{{ alert.asset_type }}</span>
            <span class="badge ms-1" :class="alert.operator === '<=' ? 'bg-danger' : 'bg-success'">
              {{ alert.operator === '<=' ? '≤' : '≥' }} ${{ alert.alert_price.toFixed(2) }}
            </span>
            <br>
            <small class="text-muted">
              <span v-if="alert.operator === '<='">
                Triggers at: <strong>${{ (alert.alert_price * 1.01).toFixed(2) }}</strong> or below (target: ${{ alert.alert_price.toFixed(2) }} + 1%)
              </span>
              <span v-else>
                Triggers at: <strong>${{ (alert.alert_price * 0.99).toFixed(2) }}</strong> or above (target: ${{ alert.alert_price.toFixed(2) }} - 1%)
              </span>
            </small>
            <br>
            <small class="text-muted" v-if="alert.last_notified_at">
              Last notified: {{ formatDate(alert.last_notified_at) }}
            </small>
          </div>
          <div class="btn-group" role="group">
            <button 
              class="btn btn-sm"
              :class="alert.is_active ? 'btn-success' : 'btn-secondary'"
              @click="toggleAlert(alert)"
              :disabled="isTogglingMap[alert.symbol]"
              :title="alert.is_active ? 'Disable alert' : 'Enable alert'"
            >
              <i class="bi" :class="alert.is_active ? 'bi-bell-fill' : 'bi-bell-slash'"></i>
            </button>
            <button 
              class="btn btn-sm btn-danger"
              @click="deleteAlert(alert.symbol, alert.asset_type)"
              :disabled="isDeletingMap[alert.symbol]"
              title="Delete alert"
            >
              <i class="bi bi-trash"></i>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- No Alerts Message -->
    <div v-else class="text-center py-4 px-3 rounded-4" style="background-color: #f8fafc; border: 1px solid #e2e8f0; color: #475569; margin-top: 0.5rem; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);">
      <i class="bi bi-bell-slash fs-3 mb-2 d-block" style="color: #94a3b8;"></i>
      <p class="mb-0 fw-semibold" style="font-size: 0.88rem; font-family: 'Outfit', sans-serif;">No active alerts for {{ symbol }}</p>
    </div>
  </div>
</template>

<script>
import { ref, computed, onMounted, watch } from 'vue';
import { useNotification } from "@kyvg/vue3-notification";

export default {
  name: 'PriceAlertWidget',
  props: {
    symbol: {
      type: String,
      required: true
    },
    assetType: {
      type: String,
      required: true,
      validator: (value) => ['crypto', 'stock', 'gold', 'silver', 'forex', 'futures'].includes(value)
    }
  },
  setup(props) {
    const { notify } = useNotification();
    
    const alerts = ref([]);
    const alertPrice = ref(null);
    const operator = ref('<=');
    const isCreating = ref(false);
    const isTogglingMap = ref({});
    const isDeletingMap = ref({});

    const activeAlertsCount = computed(() => {
      return alerts.value.filter(a => a.is_active).length;
    });

    const formatDate = (dateStr) => {
      if (!dateStr) return 'Never';
      return new Date(dateStr).toLocaleString('vi-VN');
    };

    const fetchAlerts = async () => {
      try {
        const response = await fetch(`/priceAlerts?asset_type=${props.assetType}`);
        if (response.ok) {
          const allAlerts = await response.json();
          // Filter alerts for current symbol
          alerts.value = allAlerts.filter(a => a.symbol === props.symbol);
        }
      } catch (error) {
        console.error('Error fetching alerts:', error);
      }
    };

    const createAlert = async () => {
      if (!alertPrice.value || alertPrice.value <= 0) {
        notify({
          title: "Invalid Price",
          text: "Please enter a valid price",
          type: "error"
        });
        return;
      }

      isCreating.value = true;
      try {
        const response = await fetch('/priceAlerts', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            symbol: props.symbol,
            asset_type: props.assetType,
            alert_price: parseFloat(alertPrice.value),
            operator: operator.value
          })
        });

        if (response.ok) {
          notify({
            title: "Alert Created",
            text: `Price alert set for ${props.symbol} at $${alertPrice.value}`,
            type: "success"
          });
          alertPrice.value = null;
          await fetchAlerts();
        } else {
          const error = await response.text();
          notify({
            title: "Error",
            text: error || "Failed to create alert",
            type: "error"
          });
        }
      } catch (error) {
        console.error('Error creating alert:', error);
        notify({
          title: "Error",
          text: "Failed to create alert",
          type: "error"
        });
      } finally {
        isCreating.value = false;
      }
    };

    const toggleAlert = async (alert) => {
      isTogglingMap.value[alert.symbol] = true;
      try {
        const response = await fetch(`/priceAlerts/${alert.symbol}/${alert.asset_type}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            is_active: !alert.is_active
          })
        });

        if (response.ok) {
          notify({
            title: alert.is_active ? "Alert Disabled" : "Alert Enabled",
            text: `Alert for ${props.symbol} has been ${alert.is_active ? 'disabled' : 'enabled'}`,
            type: "success"
          });
          await fetchAlerts();
        }
      } catch (error) {
        console.error('Error toggling alert:', error);
        notify({
          title: "Error",
          text: "Failed to toggle alert",
          type: "error"
        });
      } finally {
        delete isTogglingMap.value[alert.symbol];
      }
    };

    const deleteAlert = async (symbol, assetType) => {
      if (!confirm('Are you sure you want to delete this alert?')) return;

      isDeletingMap.value[symbol] = true;
      try {
        const response = await fetch(`/priceAlerts/${symbol}/${assetType}`, {
          method: 'DELETE'
        });
        
        if (response.ok) {
          notify({
            title: "Alert Deleted",
            text: "Price alert has been deleted",
            type: "success"
          });
          await fetchAlerts();
        }
      } catch (error) {
        console.error('Error deleting alert:', error);
        notify({
          title: "Error",
          text: "Failed to delete alert",
          type: "error"
        });
      } finally {
        delete isDeletingMap.value[symbol];
      }
    };

    // Watch for symbol changes
    watch(() => props.symbol, () => {
      if (props.symbol) {
        fetchAlerts();
      }
    });

    onMounted(() => {
      if (props.symbol) {
        fetchAlerts();
      }
    });

    return {
      alerts,
      alertPrice,
      operator,
      isCreating,
      isTogglingMap,
      isDeletingMap,
      activeAlertsCount,
      formatDate,
      createAlert,
      toggleAlert,
      deleteAlert
    };
  }
};
</script>

<style scoped>
.price-alert-widget {
  margin-bottom: 1rem;
}

.alert-inactive {
  opacity: 0.6;
  background-color: #f8f9fa;
}

.list-group-item {
  padding: 0.75rem 1rem;
}

.btn-group .btn {
  padding: 0.25rem 0.5rem;
}

.spinner-border-sm {
  width: 1rem;
  height: 1rem;
  border-width: 0.15em;
}
</style>
