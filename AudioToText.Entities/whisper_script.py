# whisper_script.py
# -*- coding: utf-8 -*-
import sys
import whisper
import io

# Fix encoding issue on Windows
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

def transcribe(audio_path):
    try:
        model = whisper.load_model("tiny")  # You can use "base" or larger models as needed
        result = model.transcribe(audio_path)
        print(result["text"].strip(), flush=True)  # Print only the transcription
    except Exception as e:
        print(f"Error: {str(e)}", flush=True)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Error: Missing audio path argument", flush=True)
    else:
        transcribe(sys.argv[1])
