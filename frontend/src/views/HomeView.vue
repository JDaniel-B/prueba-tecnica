<script setup lang="ts">
const apiUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api/v1'
</script>

<template>
  <main class="welcome">
    <section class="welcome__card">
      <p class="welcome__eyebrow">MesaSitec</p>
      <h1>Base del proyecto lista</h1>
      <p>
        Vue 3, TypeScript, Vue Router y Pinia están configurados. El siguiente
        incremento será el modelo de dominio y la persistencia con EF Core.
      </p>
      <dl>
        <div>
          <dt>Frontend</dt>
          <dd>http://localhost:5173</dd>
        </div>
        <div>
          <dt>API</dt>
          <dd>{{ apiUrl }}</dd>
        </div>
      </dl>
    </section>
  </main>
</template>

<style scoped>
.welcome {
  display: grid;
  min-height: 100vh;
  padding: 2rem;
  place-items: center;
}

.welcome__card {
  width: min(100%, 42rem);
  padding: clamp(2rem, 6vw, 4rem);
  border: 1px solid #dce4ef;
  border-radius: 1.5rem;
  background: #ffffff;
  box-shadow: 0 1.5rem 4rem rgb(32 55 89 / 10%);
}

.welcome__eyebrow {
  margin: 0 0 0.5rem;
  color: #2f6fed;
  font-size: 0.875rem;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

h1 {
  margin: 0;
  font-size: clamp(2rem, 6vw, 3.25rem);
  line-height: 1.05;
}

p {
  line-height: 1.7;
}

dl {
  display: grid;
  gap: 0.75rem;
  margin: 2rem 0 0;
}

dl div {
  display: grid;
  gap: 0.25rem;
  padding: 1rem;
  border-radius: 0.75rem;
  background: #f4f7fb;
}

dt {
  color: #61708a;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
}

dd {
  margin: 0;
  font-family: ui-monospace, SFMono-Regular, Consolas, monospace;
  overflow-wrap: anywhere;
}
</style>
