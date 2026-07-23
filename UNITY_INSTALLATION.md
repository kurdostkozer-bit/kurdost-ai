# 🎮 تثبيت Kurdost AI داخل Unity

## ✅ الطريقة 1: نسخ المشروع مباشرة (الأسهل)

### الخطوة 1: فتح Unity Project

1. افتح Unity
2. اذهب إلى **File → Open Project**
3. اختر أي Unity project موجود عندك

### الخطوة 2: نسخ الملفات

1. اذهب إلى: `d:\Unity-extention\kurdost-ai-final\frontend\`
2. انسخ هذه الملفات الثلاثة:
   - `KurdostAIChatService.cs`
   - `KurdostAIMainWindow.cs`
   - `KurdostAI.Editor.asmdef`

3. في Unity اذهب إلى: **Assets → Editor** (لو ما موجود Folder اسمه Editor، اعمل واحد جديد)

4. الصق الملفات الثلاثة في المجلد

### الخطوة 3: التحقق

في Unity ستشوف:
```
Assets/
└── Editor/
    ├── KurdostAIChatService.cs ✅
    ├── KurdostAIMainWindow.cs ✅
    └── KurdostAI.Editor.asmdef ✅
```

---

## ✅ الطريقة 2: استخدام Git URL (متقدم)

### في Package Manager

1. **Window → Package Manager**
2. اضغط **+** (Plus icon)
3. اختر **Add package from git URL**
4. اكتب:
   ```
   https://github.com/kurdostkozer-bit/kurdost-ai.git?path=frontend
   ```
5. اضغط **Add**

Unity بتحمّل الملفات تلقائي!

---

## ✅ الطريقة 3: استخدام .unitypackage (للتوزيع)

### إنشاء Package

1. في Unity، اختر الملفات الثلاثة
2. **Assets → Export Package...**
3. حط الاسم: `com.kurdost.ai.unitypackage`
4. Export

### تثبيت Package

في Unity project جديد:
1. **Assets → Import Package → Custom Package**
2. اختر الـ `com.kurdost.ai.unitypackage`
3. اضغط **Import**

---

## 🚀 فتح Kurdost AI Window

بعد ما تثبت الملفات:

### في Unity Editor:

1. اذهب إلى: **Window → Kurdost AI → Main**
2. بتظهر نافذة جديدة بـ 3 tabs:
   - 💬 **Chat** - للمحادثة
   - 🔧 **Tools** - أدوات
   - ⚙️ **Settings** - الإعدادات

### أول مرة (Authentication):

1. بتطلب منك API Key
2. اذهب إلى: https://console.groq.com/keys
3. احصل على Free API Key
4. الصقه في الـ window
5. اضغط **Save API Key**

---

## 💬 استخدام Chat

### إرسال Message:

1. في Tab **Chat**
2. اكتب رسالة في الـ input box
3. اضغط **Send Message**
4. بتظهر الرد من الـ AI

### الإعدادات:

في Tab **Settings**:
- 🔐 **Change API Key** - غيّر الـ key
- 🌡️ **Temperature** - تحكم الـ creativity
- 📊 **Max Tokens** - حجم الرد
- 🏥 **Health Check** - اختبر الـ backend

---

## 🔧 Troubleshooting

### المشكلة: "Window not appearing"
**الحل:**
- تأكد من المجلد: `Assets/Editor/`
- اعادة تشغيل Unity
- Check Console للـ errors

### المشكلة: "Cannot connect to backend"
**الحل:**
1. تأكد Backend شغّال: https://kurdost-ai-backend.onrender.com/health
2. Check Network Connection
3. تأكد من الـ API key صحيح

### المشكلة: "Assembly definition error"
**الحل:**
- اعادة تشغيل Unity
- Delete Library folder و اعادة فتح

---

## 📝 Quick Reference

| الشيء | المسار |
|-------|--------|
| Files | `Assets/Editor/` |
| Window | **Window → Kurdost AI → Main** |
| Backend | https://kurdost-ai-backend.onrender.com |
| API Key | https://console.groq.com/keys |
| GitHub | https://github.com/kurdostkozer-bit/kurdost-ai |

---

**لو عندك مشاكل، قول لي!** 👍
