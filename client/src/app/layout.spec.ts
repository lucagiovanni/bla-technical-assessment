import { DESKTOP_MIN_WIDTH_PX, layoutForWidth } from './layout';

describe('layoutForWidth', () => {
  it('uses a stacked layout below the desktop width', () => {
    expect(layoutForWidth(0)).toBe('mobile');
    expect(layoutForWidth(DESKTOP_MIN_WIDTH_PX - 1)).toBe('mobile');
    expect(layoutForWidth(Number.NaN)).toBe('mobile');
  });

  it('uses a desktop layout at and above the breakpoint', () => {
    expect(layoutForWidth(DESKTOP_MIN_WIDTH_PX)).toBe('desktop');
    expect(layoutForWidth(1280)).toBe('desktop');
  });
});
