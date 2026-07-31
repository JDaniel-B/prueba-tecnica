<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SolicitudForm from '@/components/SolicitudForm.vue'
import { api } from '@/services/api'
import { ApiError } from '@/lib/http'
import { useToastStore } from '@/stores/toast'
import type { Categoria, SolicitudEscritura } from '@/types/api'

const route=useRoute(), router=useRouter(), toast=useToastStore()
const id=computed(() => typeof route.params.id === 'string' ? route.params.id : '')
const editando=computed(() => Boolean(id.value))
const categorias=ref<Categoria[]>([]), inicial=ref<SolicitudEscritura>(), error=ref(''), cargando=ref(false), iniciando=ref(true)
onMounted(async()=>{try{categorias.value=await api.categorias();if(editando.value){const s=await api.solicitud(id.value);inicial.value={titulo:s.titulo,descripcion:s.descripcion,categoriaId:s.categoria.id,prioridad:s.prioridad}}}catch(e){error.value=e instanceof ApiError?e.message:'No fue posible cargar el formulario.'}finally{iniciando.value=false}})
async function guardar(body:SolicitudEscritura){cargando.value=true;error.value='';try{const s=editando.value?await api.editarSolicitud(id.value,body):await api.crearSolicitud(body);toast.mostrar(editando.value?'Solicitud actualizada.':'Solicitud creada.');await router.push(`/solicitudes/${s.id}`)}catch(e){error.value=e instanceof ApiError?e.message:'No fue posible guardar.'}finally{cargando.value=false}}
</script>
<template><main class="page"><div class="heading"><div><p>Solicitudes</p><h1>{{ editando?'Editar solicitud':'Nueva solicitud' }}</h1></div></div><p v-if="iniciando" class="loading">Cargando…</p><p v-else-if="error" class="error-box error">{{ error }}</p><SolicitudForm v-else :categorias="categorias" :inicial="inicial" :cargando="cargando" @enviar="guardar" @cancelar="router.back()" /></main></template>
<style scoped>.heading p,.heading h1{margin:.2rem 0}.heading{margin-bottom:1rem}</style>
