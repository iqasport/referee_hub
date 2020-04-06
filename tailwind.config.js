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
        }
      }
    },
    zIndex: {
      '-1': '-1'
    },
    borderRadius: {
      default: '3px',
      lg: '25px'
    }
  },
  variants: {},
  plugins: [
    require('@tailwindcss/custom-forms'),
  ]
};
