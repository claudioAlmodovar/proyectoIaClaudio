import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';

import HelloWorld from '@/components/HelloWorld.vue';

describe('HelloWorld', () => {
  it('muestra el mensaje recibido por props', () => {
    const wrapper = mount(HelloWorld, {
      props: {
        msg: 'Mensaje de prueba'
      }
    });

    expect(wrapper.text()).toContain('Mensaje de prueba');
  });
});
