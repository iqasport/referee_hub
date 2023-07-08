/* eslint-disable global-require */
module.exports = {
  theme: {
    extend: {
      colors: {
        'navy-blue': '#2F3F57',
        green: {
          default: '#72BA6C',
          lighter: '#C1EDBC',
          darker: '#286E20'
        },
        blue: {
          default: '#96C7FF',
          lighter: '#E3F0FF',
          darker: '#297EE2'
        },
        yellow: {
          default: '#FFBE45',
          lighter: '#FFE6B8',
          darker: '#D49011'
        },
        smoke: {
          default: 'rgba(0,0,0,0.5)',
          light: 'rgba(0,0,0,0.25)',
          dark: 'rgba(0,0,0,0.75)',
        }
      }
    },
    zIndex: {
      '-1': '-1',
      1: '1',
      3: '3',
      50: '50'
    },
    borderRadius: {
      default: '3px',
      lg: '25px',
      full: '9999px'
    },
  },
  variants: {},
  plugins: [
    require('@tailwindcss/custom-forms'),
  ]
};
