import sys
import whisper

def transcribe(audio_path):
    try:
        print("Loading model...", flush=True)
        model = whisper.load_model("tiny")  # Use "tiny" for faster transcription (optional)
        
        print(f"Transcribing: {audio_path}", flush=True)
        result = model.transcribe(audio_path)

        print("Transcription completed:", flush=True)
        print(result["text"], flush=True)
    except Exception as e:
        print(f"Error: {str(e)}", flush=True)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python whisper_script.py <audio_path>", flush=True)
    else:
        transcribe(sys.argv[1])
