import { mount } from '@vue/test-utils'
import { describe, expect, it } from 'vitest'
import SolicitudForm from '@/components/SolicitudForm.vue'

const categorias = [{ id: 'categoria-1', nombre: 'Incidente', slaHoras: 8 }]

describe('SolicitudForm', () => {
  it('renderiza todos los selectores obligatorios del formulario', () => {
    const wrapper = mount(SolicitudForm, { props: { categorias } })

    expect(wrapper.find('[data-testid="form-titulo"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="form-descripcion"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="form-categoria"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="form-prioridad"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="form-submit"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="form-cancelar"]').exists()).toBe(true)
  })

  it('muestra errores y no emite cuando el formulario está vacío', async () => {
    const wrapper = mount(SolicitudForm, { props: { categorias } })

    await wrapper.get('form').trigger('submit')

    expect(wrapper.get('[data-testid="error-titulo"]').text()).toContain('5 caracteres')
    expect(wrapper.get('[data-testid="error-descripcion"]').text()).toContain('10 caracteres')
    expect(wrapper.get('[data-testid="error-categoria"]').text()).toContain('categoría')
    expect(wrapper.emitted('enviar')).toBeUndefined()
  })

  it('normaliza y emite un formulario válido', async () => {
    const wrapper = mount(SolicitudForm, { props: { categorias } })
    await wrapper.get('[data-testid="form-titulo"]').setValue('  Error de acceso al portal  ')
    await wrapper.get('[data-testid="form-descripcion"]').setValue('  El portal no permite iniciar sesión desde ayer.  ')
    await wrapper.get('[data-testid="form-categoria"]').setValue('categoria-1')
    await wrapper.get('[data-testid="form-prioridad"]').setValue('Alta')

    await wrapper.get('form').trigger('submit')

    expect(wrapper.emitted('enviar')?.[0]?.[0]).toEqual({
      titulo: 'Error de acceso al portal',
      descripcion: 'El portal no permite iniciar sesión desde ayer.',
      categoriaId: 'categoria-1',
      prioridad: 'Alta',
    })
  })
})
