<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import type { Categoria, Prioridad, SolicitudEscritura } from '@/types/api'

const props = defineProps<{ categorias: Categoria[]; inicial?: SolicitudEscritura; cargando?: boolean }>()
const emit = defineEmits<{ enviar: [SolicitudEscritura]; cancelar: [] }>()
const form = reactive<SolicitudEscritura>({ titulo: '', descripcion: '', categoriaId: '', prioridad: 'Media' })
const errores = reactive({ titulo: '', descripcion: '', categoria: '' })
const prioridades: Prioridad[] = ['Critica', 'Alta', 'Media', 'Baja']
const listo = ref(false)

watch(() => props.inicial, (valor) => {
  if (valor && !listo.value) { Object.assign(form, valor); listo.value = true }
}, { immediate: true })

function submit() {
  errores.titulo = form.titulo.trim().length < 5 ? 'El título debe tener al menos 5 caracteres.' : ''
  errores.descripcion = form.descripcion.trim().length < 10 ? 'La descripción debe tener al menos 10 caracteres.' : ''
  errores.categoria = form.categoriaId ? '' : 'Selecciona una categoría.'
  if (!errores.titulo && !errores.descripcion && !errores.categoria) emit('enviar', { ...form, titulo: form.titulo.trim(), descripcion: form.descripcion.trim() })
}
</script>

<template>
  <form class="form card" @submit.prevent="submit">
    <div class="field"><label for="solicitud-titulo">Título</label><input id="solicitud-titulo" v-model="form.titulo" data-testid="form-titulo" maxlength="120" :aria-invalid="Boolean(errores.titulo)" aria-describedby="error-titulo"><small v-if="errores.titulo" id="error-titulo" class="error" data-testid="error-titulo">{{ errores.titulo }}</small></div>
    <div class="field"><label for="solicitud-descripcion">Descripción</label><textarea id="solicitud-descripcion" v-model="form.descripcion" data-testid="form-descripcion" rows="7" maxlength="4000" :aria-invalid="Boolean(errores.descripcion)" aria-describedby="error-descripcion" /><small v-if="errores.descripcion" id="error-descripcion" class="error" data-testid="error-descripcion">{{ errores.descripcion }}</small></div>
    <div class="form__grid"><div class="field"><label for="solicitud-categoria">Categoría</label><select id="solicitud-categoria" v-model="form.categoriaId" data-testid="form-categoria" :aria-invalid="Boolean(errores.categoria)" aria-describedby="error-categoria"><option value="">Selecciona…</option><option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }} · {{ c.slaHoras }} h</option></select><small v-if="errores.categoria" id="error-categoria" class="error" data-testid="error-categoria">{{ errores.categoria }}</small></div>
    <div class="field"><label for="solicitud-prioridad">Prioridad</label><select id="solicitud-prioridad" v-model="form.prioridad" data-testid="form-prioridad"><option v-for="p in prioridades" :key="p">{{ p }}</option></select></div></div>
    <div class="actions"><button type="button" class="btn btn--ghost cancel" data-testid="form-cancelar" @click="emit('cancelar')">Cancelar</button><button class="btn" data-testid="form-submit" :disabled="cargando">{{ cargando ? 'Guardando…' : 'Guardar solicitud' }}</button></div>
  </form>
</template>

<style scoped>.form{display:grid;gap:1.2rem}.form__grid{display:grid;grid-template-columns:2fr 1fr;gap:1rem}.actions{display:flex;justify-content:flex-end;gap:.7rem}.cancel{color:#25334a}@media(max-width:600px){.form__grid{grid-template-columns:1fr}}</style>
