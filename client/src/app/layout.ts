export const DESKTOP_MIN_WIDTH_PX = 720;

export type ScreenLayout = 'mobile' | 'desktop';

export function layoutForWidth(width: number): ScreenLayout {
  if (!Number.isFinite(width) || width < DESKTOP_MIN_WIDTH_PX) {
    return 'mobile';
  }

  return 'desktop';
}
