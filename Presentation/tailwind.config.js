/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.{cshtml,cs}",
    "./Views/**/*.{cshtml,cs}",
    "./Components/**/*.{cshtml,cs}",
  ],
  theme: {
    extend: {
      backdropBlur: {
        xs: '2px',
        sm: '4px',
        md: '8px',
        lg: '12px',
        xl: '16px',
      },
      backgroundOpacity: {
        '10': '0.1',
        '20': '0.2',
        '30': '0.3',
      },
      borderOpacity: {
        '10': '0.1',
        '20': '0.2',
        '30': '0.3',
      },
    },
  },
  plugins: [
    require('@tailwindcss/aspect-ratio'),
  ],
  safelist: [
    'backdrop-blur-sm',
    'backdrop-blur-md',
    'backdrop-filter',
    'bg-opacity-10',
    'bg-opacity-20',
    'border-opacity-10',
    'border-opacity-20',
    'bg-white/10',
    'bg-white/20',
    'bg-gray-800/50',
    'bg-gray-800/80',
    'filter',
    'blur-3xl',
    'mix-blend-multiply',
  ],
} 