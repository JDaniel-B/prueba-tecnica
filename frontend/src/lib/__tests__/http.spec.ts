import { afterEach, describe, expect, it, vi } from 'vitest'
import { ApiError, http } from '@/lib/http'

describe('cliente HTTP', () => {
  afterEach(() => {
    localStorage.clear()
    vi.unstubAllGlobals()
  })

  it('inyecta el token almacenado en la cabecera Authorization', async () => {
    localStorage.setItem('mesasitec_token', 'token-de-prueba')
    const fetchMock = vi.fn().mockResolvedValue(new Response(
      JSON.stringify({ estado: 'ok' }),
      { status: 200, headers: { 'Content-Type': 'application/json' } },
    ))
    vi.stubGlobal('fetch', fetchMock)

    await http<{ estado: string }>('/health')

    const llamada = fetchMock.mock.calls[0] as [string, RequestInit]
    expect(llamada[0]).toContain('/api/v1/health')
    expect((llamada[1].headers as Headers).get('Authorization'))
      .toBe('Bearer token-de-prueba')
  })

  it('convierte problem+json en un ApiError tipado', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(
      JSON.stringify({ codigo: 'VALIDACION', detail: 'Datos inválidos.' }),
      { status: 422, headers: { 'Content-Type': 'application/problem+json' } },
    )))

    const promesa = http('/solicitudes', { method: 'POST', body: '{}' })

    await expect(promesa).rejects.toBeInstanceOf(ApiError)
    await expect(promesa).rejects.toMatchObject({
      status: 422,
      message: 'Datos inválidos.',
      problema: { codigo: 'VALIDACION' },
    })
  })
})
