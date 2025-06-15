// SmartMarquee component - handles text overflow with marquee effect
const SmartMarquee = {
  props: {
    text: {
      type: String,
      required: true,
      default: ''
    },
    speed: {
      type: Number,
      default: 50 // pixels per second
    },
    gap: {
      type: Number,
      default: 50 // pixels between end and beginning of text
    }
  },
  
  setup(props) {
    const containerRef = Vue.ref(null);
    const contentRef = Vue.ref(null);
    const isOverflowing = Vue.ref(false);
    const animationDuration = Vue.ref(0);
    const contentWidth = Vue.ref(0);
    const containerWidth = Vue.ref(0);
    // Generate a unique ID for this component instance
    const uniqueId = Vue.ref(`sm-${Math.random().toString(36).substr(2, 9)}`);
    const animationName = Vue.computed(() => `marquee-${uniqueId.value}`);
    
    const checkOverflow = () => {
      if (containerRef.value && contentRef.value) {
        contentWidth.value = contentRef.value.scrollWidth;
        containerWidth.value = containerRef.value.clientWidth;
        isOverflowing.value = contentWidth.value > containerWidth.value;
        
        // Calculate animation duration based on content width and speed
        if (isOverflowing.value) {
          // Total animation distance is content width plus gap
          const animationDistance = contentWidth.value + props.gap;
          // Duration (in seconds) = distance / speed
          animationDuration.value = animationDistance / props.speed;
        }
      }
    };
    
    // Style for the text container
    const containerStyle = Vue.computed(() => {
      return {
        position: 'relative',
        width: '100%',
        height: '100%',
        overflow: 'hidden',
        whiteSpace: 'nowrap',
        display: 'block'
      };
    });
    
    // Style for the text content
    const contentStyle = Vue.computed(() => {
      if (!isOverflowing.value) {
        return {
          textAlign: 'center',
          width: '100%',
          display: 'block'
        };
      } else {
        return {
          display: 'inline-block',
          animation: `${animationName.value} ${animationDuration.value}s linear infinite`,
          whiteSpace: 'nowrap'
        };
      }
    });

    const trailingContentStyle = Vue.computed(() => {
      return {
        display: 'inline-block',
        paddingLeft: `${props.gap}px`,
        animation: `${animationName.value} ${animationDuration.value}s linear infinite`,
        whiteSpace: 'nowrap'
      };
    });

    // Create the keyframes style element for the animation
    const createKeyframesStyle = () => {
      const styleElement = document.createElement('style');
      styleElement.textContent = `
        @keyframes ${animationName.value} {
          0% { transform: translateX(0); }
          100% { transform: translateX(-${contentWidth.value + props.gap}px); }
        }
      `;
      document.head.appendChild(styleElement);
      return styleElement;
    };
    
    let styleElement = null;
    
    // Watch for changes that would affect the marquee
    Vue.watch(
      [() => props.text, () => props.speed, () => props.gap],
      () => {
        Vue.nextTick(() => {
          if (styleElement) {
            styleElement.remove();
          }
          Vue.nextTick(() => {
            checkOverflow();
            if (isOverflowing.value) {
              styleElement = createKeyframesStyle();
            }
          });
        });
      },
      { immediate: true }
    );
    
    // Check overflow on component mount and update
    Vue.onMounted(() => {
      checkOverflow();
      if (isOverflowing.value) {
        styleElement = createKeyframesStyle();
      }
      
      // Handle window resize
      window.addEventListener('resize', () => {
        checkOverflow();
        if (styleElement) {
          styleElement.remove();
        }
        if (isOverflowing.value) {
          styleElement = createKeyframesStyle();
        }
      });
    });
    
    // Clean up event listeners and style element on unmount
    Vue.onUnmounted(() => {
      window.removeEventListener('resize', checkOverflow);
      if (styleElement) {
        styleElement.remove();
      }
    });
    
    return {
      containerRef,
      contentRef,
      isOverflowing,
      containerStyle,
      contentStyle,
      trailingContentStyle
    };
  },
  
  render() {
    return Vue.h(
      'div',
      { 
        ref: 'containerRef',
        style: this.containerStyle
      },
      [
        Vue.h(
          'div',
          {
            ref: 'contentRef',
            style: this.contentStyle
          },
          this.text
        ),
        // If overflowing, add a duplicate element for seamless looping
        this.isOverflowing ? Vue.h(
          'div',
          {
            style: this.trailingContentStyle
          },
          this.text
        ) : null
      ].filter(Boolean)
    );
  }
};

// Register as global component
window.SmartMarquee = SmartMarquee;
