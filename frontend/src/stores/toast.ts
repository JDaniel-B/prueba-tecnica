import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useToastStore = defineStore('toast', () => {
  const mensaje = ref('')
  let temporizador: ReturnType<typeof setTimeout> | undefined
  function mostrar(texto: string) {
    mensaje.value = texto
    if (temporizador) clearTimeout(temporizador)
    temporizador = setTimeout(() => { mensaje.value = '' }, 3500)
  }
  return { mensaje, mostrar }
})
