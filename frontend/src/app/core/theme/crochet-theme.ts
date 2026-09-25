import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';

export const CrochetTheme = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#fcf5f2',
      100: '#f7e6df',
      200: '#eecfc3',
      300: '#e2b3a2',
      400: '#d3937c',
      500: '#b5705a',
      600: '#a1604b',
      700: '#8b5140',
      800: '#70443a',
      900: '#5c3b33',
      950: '#331e19',
    },
    colorScheme: {
      light: {
        surface: {
          0: '#ffffff',
          50: '#faf7f2',
          100: '#f4eee5',
          200: '#e8dfd2',
          300: '#d9ccba',
          400: '#b1a597',
          500: '#8a7d6f',
          600: '#6f6357',
          700: '#544a40',
          800: '#3d342c',
          900: '#2b241e',
          950: '#1a1512',
        },
      },
    },
  },
});
