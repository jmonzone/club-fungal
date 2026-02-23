#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

// UIImpactFeedbackGenerator styles
typedef NS_ENUM(NSInteger, HapticStyle) {
    HapticStyleLight = 0,
    HapticStyleMedium = 1,
    HapticStyleHeavy = 2,
    HapticStyleSoft = 3,
    HapticStyleRigid = 4
};

// Global feedback generators for each style
static UIImpactFeedbackGenerator *lightGenerator = nil;
static UIImpactFeedbackGenerator *mediumGenerator = nil;
static UIImpactFeedbackGenerator *heavyGenerator = nil;
static UIImpactFeedbackGenerator *softGenerator = nil;
static UIImpactFeedbackGenerator *rigidGenerator = nil;

extern "C" {
    
    // Initialize all haptic generators (call once at startup)
    void _InitializeHaptics() {
        if (@available(iOS 10.0, *)) {
            lightGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            mediumGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            heavyGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
            
            if (@available(iOS 13.0, *)) {
                softGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleSoft];
                rigidGenerator = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleRigid];
            }
            
            // Prepare all generators
            [lightGenerator prepare];
            [mediumGenerator prepare];
            [heavyGenerator prepare];
            
            if (@available(iOS 13.0, *)) {
                [softGenerator prepare];
                [rigidGenerator prepare];
            }
        }
    }
    
    // Trigger haptic feedback with specified style and intensity
    void _TriggerHaptic(int style, float intensity) {
        if (@available(iOS 10.0, *)) {
            UIImpactFeedbackGenerator *generator = nil;
            
            switch (style) {
                case HapticStyleLight:
                    generator = lightGenerator;
                    break;
                case HapticStyleMedium:
                    generator = mediumGenerator;
                    break;
                case HapticStyleHeavy:
                    generator = heavyGenerator;
                    break;
                case HapticStyleSoft:
                    if (@available(iOS 13.0, *)) {
                        generator = softGenerator;
                    } else {
                        generator = lightGenerator; // Fallback
                    }
                    break;
                case HapticStyleRigid:
                    if (@available(iOS 13.0, *)) {
                        generator = rigidGenerator;
                    } else {
                        generator = heavyGenerator; // Fallback
                    }
                    break;
                default:
                    generator = mediumGenerator;
                    break;
            }
            
            if (generator != nil) {
                // Intensity is currently not used by UIImpactFeedbackGenerator
                // but we keep the parameter for future use or custom implementations
                [generator impactOccurred];
                [generator prepare]; // Prepare for next use
            }
        }
    }
    
    // Check if haptics are supported on this device
    bool _HapticsSupported() {
        if (@available(iOS 10.0, *)) {
            return YES;
        }
        return NO;
    }
    
    // Cleanup haptic generators
    void _CleanupHaptics() {
        lightGenerator = nil;
        mediumGenerator = nil;
        heavyGenerator = nil;
        softGenerator = nil;
        rigidGenerator = nil;
    }
}
