# 3DFirstPersonMovement2

Bu projedeki hareket sistemi hakkındaki videom: https://youtu.be/6q3XmCUo-m0

Bu projedeki obje tutma sistemi hakkındaki videom: https://youtu.be/gGU53Ct5m6E

Projemi denemek isterseniz, "Releases" kısmında projemin çalıştırma dosyaları (Windows için .exe, Linux için .x86_64 dosyaları) bulunuyor.

Scriptlerin üst satırlarında yazmış olduğum yorumları okuyunuz. O yorumlar, Unity editörünüzde bazı şeyleri yapmanızı isteyecek.

Scriptlerin içindeki kod yorumlarının bazıları Türkçe, bazıları İngilizce haberiniz olsun.

Projemde, Unity'nin "Particle Effects" ücretsiz assetini kullandım Unity Asset Store'dan.

Projemin son versiyonunun .unitypackage dosyası, Unity'nin 6000.0.58f2 versiyonunda Universal Render Pipeline (URP) ile kullanılabilir.

Projemin ana sahnesi "BILGI_PASA" klasörünün içindeki "MovementSystemDemoScene" sahnesidir.

Projenin kodunu kendim yazdım. Vibe coding yapmadım. Ancak bazı kısımlarda internette bulduğum tutorial'lardan yardım aldığım oldu.

Kendi projelerinizde bu hareket sistemimi kullanabilirsiniz. Ama eğer ki kullanırsanız lütfen oyununuzda bu hareket sistemini kullandığınızı belirtiniz ve lütfen bu repo'yu yıldızlayınız.

Bu hareket sistemi projemin önceki versiyonuna bakmak isterseniz GitHub linki: https://github.com/BilgiPasa/3DFirstPersonMovement.git

Projemin v2.1.3'ten sonraki ve v2.0'dan önceki versiyonlarının .unitypackage dosyaları bu repo'da bulunuyor. v2.1.3 ile v2.0 versiyonları ve aralarındaki versiyonlarda .unitypackage dosyalarının bulunmamasının sebebi, o versiyonların .unitypackage dosyalarını burada paylaşmak yerine Unity Asset Store'da satmaya çalışmıştım. Fakat gerekli koşulları karşılamadığım için Asset Store'da paylaşmama izin vermediler. Zaten gerekli koşulları karşılamaya uğraşsam da uğraştığıma değmeyeceğini ve .unitypackage dosyalarını burada paylaşmamın daha iyi olacağını düşündüm. O yüzden Asset Store yerine .unitypackage dosyalarını burada paylaşmaya devam etmeye karar verdim.

Aşağıdaki üstü çizili yerde yazdığım sorun benim bilgisayarımdan kaynaklıymış. Yani önemli bişey değil.
~~ÖNEMLİ UYARI: Hareket sistemimde şöyle bir hata keşfettim. Karakteri bir süre (en az 30 saniye) W+R yaparak koşturduktan sonra, W ve R tuşarının ikisini de beraber bıraktığımda; karakter hemen kuvvet uygulamayı bırakıp yavaşlamak yerine, karakter kısa bir süre daha kuvvet uygulamaya devam edip sonra kuvvet uygulamayı bırakıp yavaşlayor ve duruyor. Yani demek istediğim, Input key'lerine uzun süre basınca Input'ta gecikme olabiliyor. Bu olayın, projemin v1.2.0 versiyonundan itibaren Unity'nin Yeni Input Sistemini kullandığımdan olabileceğini zannediyordum ama Unity'nin Legacy Input Sistemini kullandığım, projemin v1.1.14 sürümünde; hatta projemin v1.0.0 sürümünde bile bu hatanın bulunduğu fark ettim. Windows 11'de de, Linux Mint 22.2'de de, 144 fps'e kısıtlama seçiliyken de, V-Sync seçiliyken de, sınırsız fps seçeneği seçiliyken de denedim ve hala bu hatanın bulunduğunu gördüm. Bu hata nasıl çözülür bilmiyorum (ve bu sorun, projemin v2.0 sürümünde de bulunuyor). Bu problem belki Unity'den kaynaklanıyor olabilir belki de benim hatamdır, emin değilim. Haberiniz olsun diye paylaşmak istedim.~~ Bu sorun benim bilgisayarımdan kaynaklıymış :D (Bir oyunu oynarken uzun süre W'ya basmıştım ve bu sorunun o oyunda da olduğunu zannetmiştim. Oyunda uzun süre W'ya bastıktan sonrasında, yazı yazmaya çalışırken W'ya bir süre bastım. Elimi çektiğimde ise kısa bir süreliğine hala W yazmaya devam etmişti. Bu sorunu Bloodthief'te (Godot yapımı) W'ya uzun süre basınca ve Minecraft'ta (Java dilinden yapılma) W ve R'ye uzun süre basınca yaşadım. Yani bu sorun, benim bilgisayarımdan kaynaklı.)
