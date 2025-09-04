# 3DFirstPersonMovement2

Bu projedeki hareket sistemi hakkındaki videom: https://youtu.be/6q3XmCUo-m0

Bu projedeki obje tutma sistemi hakkındaki videom: https://youtu.be/gGU53Ct5m6E

Projemi denemek isterseniz, "Releases" kısmında projemin çalıştırma dosyaları (Windows için .exe, Linux için .x86_64 dosyaları) bulunuyor.

Scriptlerin üst satırlarında yazmış olduğum yorumları oku. O yorumlar, Unity editöründe bazı şeyleri yapmanı isteyecek.

Projemi yaparken ChatGPT, Deepseek, Claude gibi yapay zekaları KULLANMADIM ki zaten Generitive AI kullanmayı sevmiyorum ve tavsiye etmiyorum da.

Kodların ve yorum satırlarının hepsini kendim yazdım ama tabiki internette bulduğum tutorial'lardan yardım aldığım oldu.

Scriptlerin içindeki kod yorumlarının bazıları Türkçe, bazıları İngilizce haberin olsun.

Projemde, Ciathyza'nın "Gridbox Prototype Materials" ücretsiz assetini ve Unity'nin "Particle Effects" ücretsiz assetini kullandım Unity Asset Store'dan.

Bu hareket sistemi projemin önceki versiyonu olan "3D First Person Movement" projeme bakmak isterseniz GitHub linki: https://github.com/BilgiPasa/3DFirstPersonMovement.git

ÖNEMLİ UYARI: Hareket sistemimde şöyle bir hata keşfettim. Karakteri bir süre (en az 30 saniye) W+R yaparak koşturduktan sonra, W ve R tuşarının ikisini de beraber bıraktığımda; karakter hemen kuvvet uygulamayı bırakıp yavaşlamak yerine, karakter az bir süre daha kuvvet uygulamaya devam edip sonra kuvvet uygulamayı bırakıp yavaşlayor ve duruyor. Yani demek istediğim, Input'ta gecikme bulunuyor. Bu olayın, projemin 1.2.0 versiyonundan itibaren Unity'nin Yeni Input Sistemini kullandığımdan olabileceğini zannediyordum ama Unity'nin Legacy Input Sistemini kullandığım, projemin 1.1.14 sürümünde; hatta projemin 1.0.0 sürümünde bile bu hatanın bulunduğu fark ettim. Windows 11'de de, Linux Mint 22.1'de de, 144 fps'e kısıtlama seçiliyken de, V-Sync seçiliyken de, sınırsız fps seçeneği seçiliyken de denedim ve hala bu hatanın bulunduğunu gördüm. Bu hata nasıl çözülür bilmiyorum (ve bu sorun projemin 1.2.1 sürümünde de bulunuyor). Bu problem belki Unity'den kaynaklanıyor olabilir belki de benim hatamdır, emin değilim. Haberiniz olsun diye paylaşmak istedim.

Son olarak; "3D First Person Movement 2" projem, Unity'nin 6000.0.51f1 versiyonuna ait haberin olsun (yani Unity package'ını kullanacaksan Unity 6'da kullan).
