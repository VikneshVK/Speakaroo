#import <AVFoundation/AVFoundation.h>

extern "C" {
    void SetAudioToLoudSpeaker() {
        NSError *error = nil;
        AVAudioSession *audioSession = [AVAudioSession sharedInstance];
        
        // Set the audio session category to Playback
        [audioSession setCategory:AVAudioSessionCategoryPlayback error:&error];
        
        // Activate the audio session
        [audioSession setActive:YES error:&error];
        
        if (error) {
            NSLog(@"Error setting AVAudioSession: %@", error.localizedDescription);
        } else {
            NSLog(@"Audio session set to loudspeaker successfully.");
        }
    }
}
