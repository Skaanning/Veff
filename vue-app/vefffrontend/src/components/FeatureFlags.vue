<script setup>
import {ref, onMounted, computed} from 'vue'

const flags = ref([])
const loading = ref(true)
const error = ref(null)
const activeTab = ref('')

async function fetchFlags() {
  loading.value = true
  error.value = null
  try {
    const res = await fetch('/veff_internal_api/init')
    if (!res.ok) throw new Error('Failed to fetch flags')
    const data = await res.json()
    flags.value = data.Flags
    // Set the first tab as active by default
    if (data.Flags.length > 0) {
      activeTab.value = data.Flags[0].ContainerName
    }
  } catch (e) {
    error.value = e.message
  } finally {
    loading.value = false
  }
}

async function updateFlag(flag) {
  // Only send the properties required by FeatureFlagUpdate
  let percent = flag.Percent !== undefined ? flag.Percent : 0;
  if (flag.Type === 'BooleanFlag') {
    percent = flag.Enabled ? 100 : 0; // Boolean flags do not use Percent
  }
  const updatePayload = {
    Id: flag.Id,
    Description: flag.Description,
    Strings: flag.Strings,
    Percent: percent
  }
  try {
    const res = await fetch(`/veff_internal_api/update`, {
      method: 'PUT',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify(updatePayload)
    })
    if (!res.ok) throw new Error('Failed to update flag')
    await fetchFlags()
  } catch (e) {
    alert(e.message)
  }
}

const groupedFlags = computed(() => {
  const groups = {};
  for (const flag of flags.value) {
    if (!groups[flag.ContainerName]) {
      groups[flag.ContainerName] = [];
    }
    groups[flag.ContainerName].push(flag);
  }
  return groups;
});

onMounted(fetchFlags)
</script>

<template>
  <div class="feature-flags-layout">
    <header class="feature-flags-header">
      <h2>Feature Flags</h2>
      <div v-if="loading">Loading...</div>
      <div v-else-if="error">Error: {{ error }}</div>
      <div class="tabs">
        <button
            v-for="containerName in Object.keys(groupedFlags)"
            :key="containerName"
            :class="{ active: activeTab === containerName }"
            @click="activeTab = containerName"
        >
          {{ containerName }}
        </button>
      </div>
    </header>
    <main class="feature-flags-body">
      <div v-if="!loading && !error">
        <div v-for="(group, containerName) in groupedFlags" :key="containerName" v-show="activeTab === containerName">
          <h3>{{ containerName }}</h3>
          <table>
            <thead>
            <tr>
              <th>Name</th>
              <th>Description</th>
              <th>Type</th>
              <th>Percent</th>
              <th>Enabled</th>
              <th>Strings</th>
              <th>Actions</th>
            </tr>
            </thead>
            <tbody>
            <tr v-for="flag in group" :key="flag.Id">
              <td>{{ flag.Name }}</td>
              <td>
                <input type="text" v-model="flag.Description"/>
              </td>
              <td>{{ flag.Type }}</td>
              <td v-if="flag.Type === 'PercentageFlag'">
                <input type="number" v-model.number="flag.Percent" min="0" max="100"/>
              </td>
              <td v-else></td>
              <td v-if="flag.Type === 'BooleanFlag'">
                <input type="checkbox" v-model="flag.Enabled"/>
              </td>
              <td v-else></td>
              <td v-if="flag.Type === 'StringEqualsFlag'">
                <input type="text" v-model="flag.Strings"/>
              </td>
              <td v-else></td>
              <td>
                <button @click="updateFlag(flag)">Save</button>
              </td>
            </tr>
            </tbody>
          </table>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
</style>
