import { API_BASE } from './api-base';

describe('API_BASE', () => {
  it('points at the proxied API path', () => {
    expect(API_BASE).toBe('/api');
  });
});
